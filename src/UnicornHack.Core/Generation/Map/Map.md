# Map Fragment Authoring Guide

This document is the source of truth for authoring `MapFragment` data (the `*.cs` files
under [src/UnicornHack.Core/Data/Fragments/](../../Data/Fragments)). It describes the
glyph vocabulary, edge-condition syntax, transform behavior, tag system,
`GenerationWeight` DSL, annotations, and the relevant fragment properties — all from the
current code, not aspirational. When code and this doc disagree, the code wins; please
update the doc.

## 1. Fragment kinds

Three concrete `MapFragment` subclasses live under [src/UnicornHack.Core/Generation/Map/](.):

| Kind | File | Used for |
|---|---|---|
| `NormalMapFragment` | [NormalMapFragment.cs](NormalMapFragment.cs) | Standard chunks placed by a `RoomLayout` to fill a room's lots. |
| `ConnectingMapFragment` | [ConnectingMapFragment.cs](ConnectingMapFragment.cs) | Chunks that carry a `LevelConnection` (stairs / inter-room exits). Anonymous connection injected by `BuildChunk` when `Connections` is empty. |
| `DefiningMapFragment` | [DefiningMapFragment.cs](DefiningMapFragment.cs) | Whole-room template that overrides branch-level defaults (`RoomWidth/Height`, `CreatureGenerator`, `DefaultTerrain`, …). One per room. |

Each fragment is a public `static readonly` field on the corresponding `*Data` partial
class (`NormalMapFragmentData`, `ConnectingMapFragmentData`, `DefiningMapFragmentData`).
The reflection-based loader picks them up by class name, so the file path under
`Data/Fragments/<Kind>/` is also the lookup root.

## 2. The `Map` string

`Map` is a multi-line glyph grid. Each non-`\r\n` character is one tile. Trailing-short
rows are space-padded to the widest row. Leading `\n`s before the first row are skipped.
A fragment with `Map == ""` is a **dynamic fragment** — it must instead set `DynamicMap`
(today only `RectangleMap` exists; see [RectangleMap.cs](RectangleMap.cs)) and the
glyph-grid sections below do not apply.

### 2.1 Payload tile glyphs

These commit a `MapFeature` to the terrain when the chunk is placed. The mapping lives in
`MapFragment.GlyphToFeature` and the `Write` switch.

| Glyph | `MapFeature` | Notes |
|---|---|---|
| ` ` (space) | none — no-op | Used for row padding and intentional outside. |
| `.` | `StoneFloor` | Plain interior floor. |
| `,` | `RockFloor` | Alternate floor (visually distinct, walkable). |
| `#` | `StoneWall` | Perimeter wall. Yields a doorway candidate via random fallback when no `+` is authored. |
| `A` | `StoneArchway` | Walkable wall replacement; counts as perimeter. |
| `~` | `Pool` | Authored pool tile **inside** the payload. At the four payload corners `~` has a different meaning (see §3). |
| `+` | doorway candidate | Perimeter doorway. Counted into `PossibleDoorwayCount`. Auto-emits `door-n/s/e/w` tags by edge. |
| `^` | trap point | Recorded into `Chunk.TrapPoints` (materialization deferred). Auto-tag `trap`. |
| `<` | connection | Connection up (entrance/up stairs). Picks a `LevelConnection` whose `Glyph == '<'`, or an anonymous one when none authored. Auto-tag `entrance`. |
| `>` | connection | Connection down (exit/down stairs). Auto-tag `exit` (suppressed when the fragment is authored with the `shortcut` role tag — see §5). |
| `%` | creature point | Recorded into `Chunk.CreaturePoints` so `CreatureGenerator` spawns a difficulty-appropriate creature here. |
| `$` | wildcard feature point | Bound at chunk placement time by `BindWildcardFeatures` to one of the room's still-outstanding placeable features. Auto-tag `wildcard-feature`. |

### 2.2 Feature glyphs

`MapFragment.FeatureGlyphs` maps each glyph to a `RoomFeatures` bit. The underlying tile
resolves to the feature's `MapFeature` (see `RoomFeatureToMapFeature`); `Chunk.FeaturePoints`
records the binding so room finalization can wire up entity behavior later.

| Glyph | `RoomFeatures` | Generic tag | Specific tag |
|---|---|---|---|
| `_` | `DeityAltar` | `altar` | `deity-altar` |
| `\` | `MajorShrine` | `shrine` | `major-shrine` |
| `/` | `MinorShrine` | `shrine` | `minor-shrine` |
| `&` | `RitualShrine` | `shrine` | `ritual-shrine` |
| `{` | `SmallFountain` | `fountain` | `small-fountain` |
| `}` | `BigFountain` | `fountain` | `big-fountain` |
| `?` | `SkillPedestal` | `pedestal` | `skill-pedestal` |
| `!` | `RacePedestal` | `pedestal` | `race-pedestal` |
| `"` | `TraitPedestal` | `pedestal` | `trait-pedestal` |
| `=` | `FeatPedestal` | `pedestal` | `feat-pedestal` |
| `[` | `ArmorStand` | `stand` | `armor-stand` |
| `]` | `GildedArmorStand` | `stand` | `gilded-armor-stand` |
| `(` | `ItemCase` | `stand` | `item-case` |
| `:` | `GildedItemCase` | `stand` | `gilded-item-case` |
| `)` | `WeaponStand` | `stand` | `weapon-stand` |
| `;` | `GildedWeaponStand` | `stand` | `gilded-weapon-stand` |
| `^` | `Trapped` | `trap` | `trapped` (see §5.1) |

The non-placeable bits (`Secret`, `Cursed`, `Challenge`, `Boss`) **have no glyph**. A
fragment opts into one of those rooms by adding the canonical tag (`secret`, `cursed`,
`challenge`, `boss`) to `Tags`, or opts out via `!cursed` etc. See §5.

### 2.3 Annotations (per-cell glyph randomization)

`Annotations: Dictionary<char, string>` lets a fragment author one cell as "pick one of
these glyphs at placement time". Resolution happens once per chunk placement using
`room.Level.GenerationRandom`. Any glyph may be annotated, including standard terrain
glyphs.

```csharp
Map = @"
###
#1#
###",
Annotations = new Dictionary<char, string> { ['1'] = "._" }   // 50/50 plain floor vs deity altar
```

Constraints:

- Substitution string must be **non-empty**; load throws `InvalidOperationException` otherwise.
- The annotation key is itself written into `ByteMap`, then replaced inside `Write`. It does
  not need to be a defined tile glyph.

Numeric glyphs `0`–`9` are reserved by convention for annotations, but the parser doesn't
enforce that.

## 3. Conditional edges (the `~` corner marker)

A `~` placed in a **payload corner** is one half of a marker pair — an edge becomes a
*conditional edge* (metadata describing what the fragment expects on the neighbor side)
only when **both** of its corners carry `~`. A single-corner `~` is treated as an
authored Pool tile at that corner.

Each conditional edge sets the matching `ConditionalFirstRow` / `ConditionalLastRow` /
`ConditionalFirstColumn` / `ConditionalLastColumn` flag and shrinks `PayloadArea`
inward by one tile on that side. The corner cells themselves are NOT edge slots — the
parsed condition array spans only the non-corner cells of the conditional row / column.

After `EnsureInitialized`, the parsed conditions live in the four
`NorthEdge` / `SouthEdge` / `EastEdge` / `WestEdge` arrays of `EdgeCondition` (length =
`PayloadArea` width or height — see [EdgeCondition.cs](EdgeCondition.cs)).

### 3.1 Edge-condition vocabulary

Inside a conditional row/column each cell uses one of these symbols:

| Symbol | `EdgeConditionKind` | Meaning (constraint on the cell *outside* the payload, one step beyond this edge) |
|---|---|---|
| ` ` (space) | `Default` | No constraint. |
| `S` | *resolved at parse time* | Mirror condition: replaced during parsing with the tile-specific `EdgeConditionKind` that matches the payload's adjacent edge-tile feature (e.g. `S` next to a `.` becomes `StoneFloor`; next to a `#` becomes `StoneWall`). The adjacent payload glyph must have a concrete `MapFeature` — a doorway-candidate / wildcard / trap / creature / padding glyph throws at load. |
| `F` | `Filled` | Neighbor cell must already be a non-`Default` tile (anything previously written). |
| `U` | `Unfilled` | Neighbor cell is still `Default` (empty space inside the room or outside the room). |
| `E` | `Edge` | The payload edge tile must coincide with the room boundary (neighbor lies outside the room). |
| `#` | `StoneWall` | Neighbor cell must equal `StoneWall`. |
| `,` | `RockFloor` | Neighbor cell must equal `RockFloor`. |
| `.` | `StoneFloor` | Neighbor cell must equal `StoneFloor`. |
| `+` | `Doorway` | Neighbor cell must be `StoneFloor` or `StoneArchway` — mirrors the payload `+` doorway-glyph semantic so a payload-edge `+` aligns with a walkable threshold on the other side. |
| `A` | `StoneArchway` | Neighbor cell must equal `StoneArchway`. |

Any other character (including feature glyphs like `_` or annotation glyphs) throws
`InvalidOperationException` at load time — feature semantics belong in the payload, not
in edge metadata.

### 3.2 Edge condition examples

```text
~....~      // top-left & top-right corners → top row conditional; payload is the bottom row
#####       // payload row 1: a 5-wide wall
```

The first row above is metadata; the parsed `NorthEdge` is `[Default, Default, Default, Default]`
covering the 4-wide payload (corners trimmed).

```text
~SSSS~
#####
.....
#####
~FFFF~
```

Here `NorthEdge` is `[StoneWall, StoneWall, StoneWall, StoneWall]` (the `S` cells
resolved against the row-1 `#####` payload above them) and `SouthEdge` is
`[Filled, Filled, Filled, Filled]`. The payload is the middle three rows.

### 3.3 How edges influence weight

`GetEdgeMatchMultiplier(room, ctx)` evaluates a fragment against a candidate `LotPlacementContext`:

- Returns **0** when the fragment cannot be placed at any anchor inside the lot satisfying
  every active condition (size mismatch on a forced-flush axis, every condition violated,
  etc.).
- Otherwise returns `1 + matched * 0.1f` where `matched` is the number of successfully
  resolved non-Default conditions. The `+0.1` is a *tie-breaker bias* — fragments with
  more aligned edges beat blander ones at equal authored weight.

A conditional edge participates in evaluation when the fragment is forced to sit flush
with that side of the lot. The current rule (`ComputeStickFlags`) sticks a side when any
of the following holds: (i) the payload dimension matches the lot in that axis; (ii)
`LotPlacementContext.TargetEdge` equals that side; or (iii) the fragment has a
conditional edge on that side AND the side is *closed* (no room outside the lot for
another lot). Conditional-but-open sides and unconditional sides stay relaxed; their
conditions (if any) are carried forward by `RegisterPreservedConditions` and re-evaluated
against whichever chunk lands on the cell next, or filled in by `FixupDeferredConditions`
at the end of room layout when no chunk does.

## 4. Transformations (D4 variants)

By default each fragment is materialized into the full **dihedral D4 orbit** — up to 8
variants: identity, 3 rotations (90°/180°/270°), and 4 reflections. Variants are produced
eagerly by `MapFragmentLoader<T>.Load` calling `MapFragment.CreateVariant(transform)`,
which `MemberwiseClone`s the source and rewrites its byte map, payload area, conditional
edges, and per-cell edge condition arrays.

| Flag | Effect |
|---|---|
| `NoMirror` | Skip the 4 reflections; keep the 4 pure rotations only. |
| `NoTurnOver` | The payload has full C4 rotational symmetry; keep identity + one flip only. |
| `NoTransform` | No variants at all; keep only the identity. Implies the other two. |
| `NoOverwrite` | Layout flag (see §6); inherited by variants but unrelated to orbit size. |

The orbit size is exposed as `VariantCount`; per-variant `WeightMultiplier = 1 / VariantCount`
is applied automatically inside `GetWeight`, so the total pick probability across all
variants equals the authored `GenerationWeight`.

Variants carry:

- A unique `Name` suffixed with the transform name (e.g. `myRoom_Rotate90`).
- A shared `SourceName` (the source's `Name`), used for cross-variant instance accounting.
- A re-derived `Tags` set: `door-*` tags are stripped from the source's tag set and
  re-emitted from the rotated payload by `ApplyTagDerivations`. `AntiTags` are shared by
  reference — opt-outs are author declarations on the family, not per-orientation.

The loader does **not** deduplicate identical variants by geometry. If your payload is
symmetric, set the matching flag explicitly.

## 5. Tags

`Tags: ICollection<string>` describes what a fragment *provides* and informs the
demand-aware weight bonus. After `EnsureInitialized` the collection contains:

1. **Author positives** — every entry without a `!` prefix, trimmed, case-insensitive.
2. **Author opt-outs** — entries prefixed with `!` are moved into `AntiTags` and the `!`
   stripped. An empty `!` entry throws.
3. **Auto-derived tags** (`MapFragment.ApplyTagDerivations`):
   - `size-small` if payload tile count ≤ 15, `size-medium` if ≤ 49, else `size-large`.
   - Generic content tag for each feature/connection/trap glyph in the payload (`altar`,
     `shrine`, `fountain`, `pedestal`, `stand`, `entrance`, `exit`, `trap`,
     `wildcard-feature`).
   - Specific feature tag for each feature glyph (`small-fountain`, `weapon-stand`, …) —
     see `FeatureTagNames`.
   - `door-n` / `door-s` / `door-e` / `door-w` for each payload edge that hosts at least
     one `+` doorway glyph. Re-derived per variant by `CreateVariant`.

### Connection tags (`entrance` / `exit` / `shortcut`)

The three connection-sourcing roles each have a canonical tag in `FeatureTagNames`. A
`ConnectingMapFragment` that carries the tag matching the room's current need (`entrance`
while receiving a dangling incoming connection, otherwise `exit` for an exit room or
`shortcut` for a hidden shortcut room) gets a selection-weight bonus (`RoleTagWeightBonus`
in `UniformRoomLayout`) — but an **untagged** connecting fragment stays selectable for any
role, so the tag is a preference, never a requirement. `entrance` / `exit` are auto-derived
from the `<` / `>` glyphs; `shortcut` has no exclusive glyph and must be authored explicitly. When a
fragment is authored `shortcut`, the auto `exit` tag from a `>` glyph is **suppressed** so a
shortcut fragment that draws down-stairs for flavour isn't mistaken for a general exit
fragment.

The full canonical tag list for `RoomFeatures` is in `MapFragment.FeatureTagNames`.

### 5.1 Demand-aware weight

`GetDemandMultiplier(room, ctx)` tracks a single signed match counter and scales the
weight linearly from it:

- A **demand match** increments the counter: an outstanding placeable `RoomFeatures` bit
  whose canonical tag is in `Tags` (or an outstanding non-placeable bit; or a cardinal
  direction in `ctx.DemandedDoorways` whose `door-*` tag is in `Tags`).
- An **anti-tag match** decrements the counter: an outstanding feature/door whose tag is
  in `AntiTags`.
- The final multiplier is `1 + n * (DemandTagBonus - 1)` for a non-negative net `n`, or
  `1 / (1 + |n| * (DemandTagBonus - 1))` for a negative net — so one net demand match
  multiplies the weight by `DemandTagBonus` and one net anti-tag match divides by
  it. Matches accumulate linearly, not exponentially.
- A `Tags` entry of `wildcard-feature` matches every outstanding placeable bit (bonus
  applied per outstanding bit not already matched directly). Wildcard does not cover
  non-placeable bits.
- **Hard-exclusion**: a fragment whose `Tags` advertise a placeable feature the room
  did not demand is rejected outright (multiplier `0`) so a themed fragment never
  saddles an unwilling room with its contents.
- **Trapped exemption**: `RoomFeatures.Trapped` opts out of the hard-exclusion. A
  fragment carrying the `trapped` tag (one or more `^` glyphs) is allowed in any room;
  if the room did not demand `Trapped` the weight is multiplied by
  `UndemandedTrapPenalty = 0.25f` instead of being rejected. The `Trapped` demand is
  also never marked satisfied by `BindWildcardFeatures` (concrete `^` bits are folded
  into `BoundPlaceableFeatures` for every other feature but not for `Trapped`), so
  multiple trap-bearing fragments can land in a `trapped`-demanding room.

An "outstanding" placeable bit is one set on `room.Features` but not yet in
`room.BoundPlaceableFeatures`. Non-placeable bits (`Secret` / `Cursed` / `Challenge` /
`Boss`) are always considered outstanding when set; `Trapped` is also always
considered outstanding (see exemption above).

## 6. `GenerationWeight` DSL

`GenerationWeight` is a string parsed by `UnicornExpressionVisitor` into a `Func`. The
script returns a `float`; the engine multiplies it by the edge multiplier (§3.3), demand
multiplier (§5.1), and per-variant `WeightMultiplier` (§4) before normalising across
candidates.

### 6.1 Parameters

| Token | Type | Source |
|---|---|---|
| `$branch` | `string` | `room.Level.Branch.Name` (e.g. `"dungeon"`, `"surface"`, `"entrance"`). |
| `$depth` | `int` | `room.Level.Depth`. |
| `$instances` | `int` | Number of times this source fragment (across all variants) has been placed on this level. From `GenerationStats.FragmentInstancesByName[SourceName]`. |
| `$tagInstances` | `IReadOnlyDictionary<string,int>` | Per-tag placement count on this level. Use indexer: `$tagInstances["altar"]`. Missing keys return `0`. |
| `$roomFeatures` | `RoomFeatures` | The owning room's feature bitmask (only on `DefiningMapFragment`-flavoured calls). |

### 6.2 Operators & built-ins

- Arithmetic: `+ - * / %`.
- Comparison and equality: `== != < <= > >=`.
- Boolean: `&& || !`.
- Ternary: `cond ? a : b`.
- `Infinity` for "always pick".
- Functions: `Min(a, b, …)`, `Max(a, b, …)`. Other identifiers resolve through the
  visitor — check [UnicornExpressionVisitor.cs](../UnicornExpressionVisitor.cs) for the
  current list.

### 6.3 Examples

```csharp
GenerationWeight = "$branch == 'surface' ? Infinity : 0"     // SurfaceRoom-only
GenerationWeight = "$branch == 'dungeon' || $branch == 'entrance' ? 1 : 0"
GenerationWeight = "$instances == 0 ? 5 : 0"                  // exactly once per level
GenerationWeight = "$depth > 5 && $tagInstances[\"altar\"] == 0 ? 3 : 1"
```

If `GenerationWeight` is `null` or absent, the implicit weight is `1`.

## 7. Other fragment properties

These are on `MapFragment` unless otherwise noted.

| Property | Default | Use |
|---|---|---|
| `Name` | required | Unique identifier across loaders of the same kind. |
| `Map` | `""` | Glyph grid (see §2). Empty = dynamic fragment; requires `DynamicMap`. |
| `DynamicMap` | `null` | Dynamic-shape provider (e.g. `RectangleMap { MinSize = ... }`). |
| `Connections` (on `ConnectingMapFragment`) | empty | `LevelConnection` definitions matched to `<` / `>` glyphs by `Glyph`. Empty list ⇒ anonymous connection injected by `BuildChunk`. |
| `NoRandomDoorways` | `false` | When `true`, no `?`-fallback doorways are added — only authored `+` glyphs count. Set this when the fragment authors doorways (or chooses not to have any). |
| `NoMirror` / `NoTurnOver` / `NoTransform` / `NoOverwrite` | `false` | See §4 and §6 below. |
| `RoomWidth` / `RoomHeight` / `CreatureGenerator` / `ItemGenerator` / `DefaultTerrain` / `DefaultPathTerrain` (on `DefiningMapFragment`) | `null` | Per-room overrides of the branch defaults. `null` = inherit. |

### 7.1 `NoOverwrite`

`DenseRoomLayout` may shift a candidate lot one tile toward an adjacent placed chunk so
their perimeter walls collapse into a single shared column (wall-merge). Setting
`NoOverwrite = true` on a fragment forbids any neighbor from shifting onto **this** chunk
afterward — useful for vaults, framed shrines, and altar rooms that depend on exact wall
thickness. Default `false` preserves the old `UniformRoomLayout` behavior and lets
dense layouts share walls freely.

## 8. Authoring checklist

When adding or editing a fragment:

1. Decide kind (`Normal` / `Connecting` / `Defining`) and put the file under
   `Data/Fragments/<Kind>/`.
2. Pick glyphs from §2.1 / §2.2 only — anything else needs an `Annotations` entry or
   it'll be parsed as `predefinedPoints` (creature/entity placeholder; rarely what you
   want).
3. If you use a conditional edge (`~`-corner), every cell in that edge row/column must be
   one of the symbols in §3.1 — feature glyphs in an edge row throw at load time. An
   `S` edge cell additionally requires the adjacent payload tile to be a concrete-feature
   glyph (terrain or a feature with a `MapFeature` mapping); placing `S` next to a
   doorway-candidate / wildcard / trap / creature / padding glyph throws.
4. Decide transform flags. Asymmetric → leave default (full D4). Reflection-symmetric →
   `NoMirror`. C4-symmetric → `NoTurnOver`. Hand-tuned single variant → `NoTransform`.
5. Audit tags. Auto-tags cover size, content glyphs, and (per-variant) `door-*`. Add the
   canonical non-placeable feature tag (`secret` / `cursed` / `challenge` / `boss`) only
   when the fragment is built for that room kind. Add `!{feature}` opt-outs where the
   fragment must never appear.
6. Set `NoRandomDoorways = true` if you've authored exactly the doorways you want (or
   none).
7. Set `NoOverwrite = true` if exact wall thickness matters.
8. Test with `dotnet test test\UnicornHack.Core.Tests` — the fragment-loader and weight
   tests catch most authoring errors at load.

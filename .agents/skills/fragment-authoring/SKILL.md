---
name: fragment-authoring
description: 'Author or edit UnicornHack map fragments — the glyph-grid templates that level generation stitches into rooms. Use when: creating a new `NormalMapFragment` / `ConnectingMapFragment` / `DefiningMapFragment`, editing an existing fragment under `src/UnicornHack.Core/Data/Fragments/`, adding edge conditions or annotations, choosing transform flags, writing `GenerationWeight` DSL expressions, picking feature/connection glyphs, or wiring per-tile creature wildcards (`%`) and feature wildcards (`$`).'
argument-hint: 'Describe the fragment to create or change (e.g. "add a 7x7 altar room", "edit DungeonRoom to use rock floor", "make this fragment surface-branch only")'
---

# Fragment Authoring

Use this skill when working on map fragments — the data files that drive procedural
level generation. The full reference lives in
[src/UnicornHack.Core/Generation/Map/Map.md](../../../UnicornHack/src/UnicornHack.Core/Generation/Map/Map.md);
this skill points you at the right sections and patterns.

## Procedure

1. **Read [Map.md](../../../src/UnicornHack.Core/Generation/Map/Map.md)** —
   it documents the glyph vocabulary, edge-condition syntax, transforms, tags,
   `GenerationWeight` DSL, and annotations from the current code. Treat it as the source
   of truth.
2. Decide the fragment kind and file location:
   - `NormalMapFragment` → `src/UnicornHack.Core/Data/Fragments/Normal/`
   - `ConnectingMapFragment` (carries a `LevelConnection`) → `Data/Fragments/Connecting/`
   - `DefiningMapFragment` (whole-room override) → `Data/Fragments/Defining/`
   Each fragment is a `public static readonly` field on the matching
   `*Data` partial class — see the existing files for the pattern.
3. Pick glyphs only from Map.md §2.1 / §2.2 / §2.3. Anything else triggers the
   `predefinedPoints` branch (entity placeholder; usually not what you want) or, in a
   conditional edge row, throws at load.
4. If using `~`-corner conditional edges, every cell in that row/column must use one of
   the edge-condition symbols (Map.md §3.1).
5. Set transform flags (`NoMirror`, `NoTurnOver`, `NoTransform`) based on the payload's
   symmetry. Default = full D4 orbit (8 variants); the loader does **not** dedup by
   geometry, so symmetric payloads need the matching flag.
6. Set `NoRandomDoorways = true` when you author the doorways explicitly or want none.
7. Set `NoOverwrite = true` when exact wall thickness matters (vaults, framed shrines).
8. Use `Tags` to add `secret` / `cursed` / `challenge` / `boss` for non-placeable room
   features, and `!{feature}` to opt out. Most tags are auto-derived — don't duplicate the
   auto set.
9. Validate with `dotnet test test\UnicornHack.Core.Tests --nologo` — loader and weight
   tests catch most authoring errors at load.

## Quick reference

### Tile glyphs (§2.1 of Map.md)

`.` floor · `,` rock floor · `#` wall · `A` archway · `~` pool (corner = edge marker) ·
`+` doorway · `^` trap · `<` stairs up · `>` stairs down · `%` creature wildcard ·
`$` feature wildcard.

### Feature glyphs (§2.2 of Map.md)

`_` altar · `\` major shrine · `/` minor shrine · `&` ritual shrine · `{` small fountain
· `}` big fountain · `?` skill pedestal · `!` race pedestal · `"` trait pedestal ·
`=` feat pedestal · `[` armor stand · `]` gilded armor stand · `(` item case ·
`:` gilded item case · `)` weapon stand · `;` gilded weapon stand.

### Edge symbols (§3.1 of Map.md)

` ` Default · `S` Same · `F` Filled · `U` Unfilled · `E` Edge · `#`/`,`/`.`/`A`
TileSpecific · `+` Doorway (matches `StoneFloor` or `StoneArchway`)
(note: in an edge row `.` is **not** Default).

### DSL parameters (§6.1 of Map.md)

`$branch` (string) · `$depth` (int) · `$instances` (int) ·
`$tagInstances["tag-name"]` (dictionary) · `$roomFeatures` (RoomFeatures)

## When to bail and ask

- If the desired effect needs a glyph that isn't in Map.md — propose adding one to
  `FeatureGlyphs` or to the parser.
- If a new DSL parameter is needed (something `UnicornExpressionVisitor` doesn't expose
  yet), surface that as a separate change.
- If the placement engine would need a new condition (e.g. a per-region constraint, not
  per-edge), surface that as a separate change.

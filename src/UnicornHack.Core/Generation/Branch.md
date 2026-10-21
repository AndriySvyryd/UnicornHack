# Branch Authoring Guide

This guide describes branch definitions under [Data/Branches](../Data/Branches),
their generation expressions, level layouts, room defaults and population settings.
It documents the current implementation. For fragment glyphs, transforms and room
geometry, use the [Map Fragment Authoring Guide](Map/Map.md).

## 1. Definition and identity

Each branch is a public `static readonly Branch` field on the partial
`UnicornHack.Data.Branches.BranchData` class. Its C# field name identifies the
generated source member; `Name` is the runtime lookup key. Use unique ASCII names,
normally lowercase words separated by underscores. References must use the exact
runtime name; old branch IDs are not aliases.

```csharp
namespace UnicornHack.Data.Branches;

public static partial class BranchData
{
    public static readonly Branch SampleBranch = new Branch
    {
        Name = "sample_branch",
        Length = 3,
        GenerationWeight = "$branch == 'outer wards' ? 1 : 0",
        IsTerminal = true,
        PerimeterConnections = true,
        Layout = new GridLevelLayout
        {
            GridWidth = 7,
            GridHeight = 5,
            MinRooms = 28,
            MaxRooms = 32,
            Interconnectedness = 0.75F,
            RewardWeights = new Dictionary<RoomFeatures, string>
            {
                [RoomFeatures.TraitPedestal] = "0",
                [RoomFeatures.FeatPedestal] = "0"
            }
        },
        RoomLayout = new DenseRoomLayout()
    };
}
```

This is an authoring example, not a complete new content pool. Also author eligible
defining/normal fragments and creature/item expressions for `sample_branch`.
The branch is loaded by [Branch.Loader](Branch.cs); no generation allowlist is needed.

## 2. Branch properties

| Property | Default | Meaning |
| --- | --- | --- |
| `Name` | required | Unique ASCII runtime ID. |
| `Length` | `0` | Set a positive number of levels for a playable branch. Depths start at 1. |
| `Difficulty` | `0` | Branch difficulty input used when creating levels; see [LevelGenerator](LevelGenerator.cs). |
| `GenerationWeight` | implicit `"1"` | Weight when choosing this branch as an inter-branch destination. |
| `IsTerminal` | `false` | At its terminal depth, a terminal branch has no ordinary onward exit. |
| `Layout` | `new EmptyLevelLayout()` | Builds the room graph for each level. |
| `RoomLayout` | `new UniformRoomLayout()` | Arranges fragments within each room. |
| `RoomWidth`, `RoomHeight` | `25`, `16` | Default tile dimensions of each room, not the room-graph grid. |
| `PerimeterConnections` | `false` | Extend inter-room doorways to matching room boundaries. |
| `CreatureGenerator` | expected 8, maximum 255 | Default per-room creature population settings. |
| `ItemGenerator` | expected 3 | Default per-room item population settings. |
| `DefaultTerrain` | `MapFeature.Default` | Initial terrain fill. |
| `DefaultPathTerrain` | `MapFeature.RockFloor` | Terrain used for generated paths. |
| `DefaultWall` | `MapFeature.RockWall` | Fallback generated wall material. |

Defining fragments may override room dimensions, population generators and terrain
defaults. They do not replace the branch's level-graph layout. See
[DefiningMapFragment](Map/DefiningMapFragment.cs) for the effective-value rules.

## 3. Destination weights and connections

Branch `GenerationWeight` receives the **source** level's context:

| Parameter | Type | Meaning |
| --- | --- | --- |
| `$branch` | string | Source branch runtime ID. |
| `$depth` | int | Source level depth. |

```csharp
GenerationWeight = "$branch == 'outer wards' ? 1 : 0"
GenerationWeight = "$branch == 'sample_branch' && $depth >= 3 ? 2 : 0"
GenerationWeight = "0"
```

A branch cannot select itself as the next branch, regardless of its expression.
An anonymous downward connection advances one depth within the branch; a Shortcut
advances two. If that exceeds the branch length, the generator chooses a destination
from positive branch weights. No eligible destination is a generation error, so give
every non-terminal boundary a valid next branch.

A connecting fragment can specify a `LevelConnection.BranchName` to bypass random
branch selection. Cross-branch destinations default to depth 1 unless `Depth` is
authored. An explicitly named same-branch connection advances to the next depth.
See [LevelConnection](Map/LevelConnection.cs) for receiving-connection matching.

## 4. Level graphs versus room layouts

- `EmptyLevelLayout` creates a single-room level; it is often paired with an
  `EmptyRoomLayout` and a defining map.
- `GridLevelLayout` uses cardinal grid adjacency.
- `TreeLevelLayout` uses eight-neighbor adjacency and its own room placement policy.
- `UniformRoomLayout` and `DenseRoomLayout` control per-room fragment placement,
  not inter-room topology. Dense defaults include 0.85 coverage, at most 12 fragments
  and a minimum lot of 4x4.

The common graph settings live on [LevelLayout](Map/LevelLayout.cs):

| Setting | Common default | Meaning |
| --- | --- | --- |
| `GridWidth`, `GridHeight` | 7, 5 | Logical room-grid dimensions. |
| `MinRooms`, `MaxRooms` | 28, 32 | Successful graph room-count bounds. Generation retries undersized attempts. |
| `Interconnectedness` | 0.75 | Additional connection density. |
| `Sinuosity` | 0 | Entrance-to-exit path winding; high values cost more to generate. |
| `RewardRoomProbability` | 0.3 | Visible non-endpoint reward chance, boosted for low-degree rooms. |
| `CurseRoomProbability` | 0.1 | Non-endpoint curse chance. |
| `HiddenConnectionProbability` | 0.3 | Expected concealed-room count, not a per-room probability. |

`TreeLevelLayout` changes reward/curse/hidden defaults to 0.4, 0.15 and 0.1.
Its other settings can still be overridden in the branch definition.

The graph maintains forward reachability from the entrance and to the exit. Hidden
rooms stay off the chosen spine and retain valid routes. Premium reward features
are excluded from every shortest directed entrance-to-exit path.

With `PerimeterConnections`, ordinary doorways may be extended through unprotected
plain terrain. The extension avoids existing connections, protected fragments,
corner destinations and intermediate boundary tiles. It does not move an authored
exact-spawn anchor; the creature generator separately relocates anchors inside arrival safety zones.

## 5. Reward expressions

`Layout.RewardWeights` maps individual `RoomFeatures` values to string expressions.
Only write entries that differ from the defaults. Missing entries keep the default
caps, difficulty scaling, repeat throttling and hidden-room bias.

| Parameter | Type | Meaning for the feature being evaluated |
| --- | --- | --- |
| `$count` | int | Placements on the current level. |
| `$globalCount` | int | Placements across the game, including the current level so far. |
| `$difficulty` | int | Effective level difficulty, not branch depth. |
| `$hidden` | bool | Whether the candidate room is concealed. |

```csharp
RewardWeights = new Dictionary<RoomFeatures, string>
{
    [RoomFeatures.DeityAltar] = "$count == 0 ? 32.0 : 0.0",
    [RoomFeatures.BigFountain] = "$hidden && $difficulty >= 3 ? 12.0 / (1 + $globalCount) : 0.0",
    [RoomFeatures.FeatPedestal] = "0"
}
```

Each override is the **complete** selection weight: no default repeat or hidden
multiplier is applied afterward. Results must be finite and nonnegative. Integer
division remains integer division; use decimal literals when a fractional result
is intended. Compiled expressions are cached by expression text.

The default Shortcut expression returns zero for visible rooms. Hidden rooms always
attempt a reward pick; visible reward chance is controlled separately by
`RewardRoomProbability`. Premium shortest-path exclusion still applies independently
of authored weights. See the default expressions in [LevelLayout](Map/LevelLayout.cs).

## 6. Fragment and population eligibility

Fragment and creature/item weights receive their own context; they are not branch
destination weights. Fragment `$branch` and `$depth` describe the level being filled.
Defining fragments can test the implicit room-feature mask:

```csharp
GenerationWeight = "$branch == 'sample_branch' && HasFeature('Boss') ? 1 : 0"
```

`HasFeature('Boss')` uses the implicit room-feature context. The explicit form is
`HasFeature($roomFeatures, 'Boss')`; an explicit numeric mask also works without
an implicit context. Ordinary normal-fragment and branch-destination expressions
do not provide `$roomFeatures`. Other feature names use the same helper, such as
`HasFeature('RacePedestal')`.

Every authored creature explicitly declares branch eligibility or `"0"`; the
framework's unconfigured default is `"1.0"`. There is no branch creature allowlist.
Named-creature uniqueness is tracked by placed-instance counts, with one named
non-boss slot per level. Bosses are exempt from that per-level slot limit.

`CreatureGenerator.ExpectedInitialCount` is a stochastic target, not a hard maximum.
Use `MaximumInitialCount` for a cap. Exact `Creatures` anchors bypass weighted
creature selection but still obey occupancy, arrival protection, maximum count and
uniqueness. An exact anchor inside an arrival safety zone is moved to the nearest
free walkable chunk cell outside it before the spawn attempt; the selected creature
identity is preserved. If no legal cell exists, placement fails rather than
silently substituting another creature. Item generation uses its existing item groups
and item weight expressions.

Boss rooms use the same weighted defining-fragment selection as every other room.
Every graph layout marks its exit room with `Boss`; there is no opt-in setting.
Every playable branch therefore needs an eligible boss-tagged exit definition.
The picker requires the `boss`
tag for boss rooms and excludes that tag for ordinary rooms before evaluating
weights. Boss formulas therefore only need branch/depth tests and ordinary instance
counts, not `HasFeature('Boss')`; generic defining rooms need no inverse check.
Use `$globalInstances == 0` for a hall that may appear only once per game.
There is no separately stored or preselected boss fragment. Retry rollback restores
the same fragment-count bookkeeping used by ordinary rooms; saved generated rooms
retain their actual actors and terrain.

Defining-fragment weights also apply the shared room-feature tag and anti-tag
multiplier. Authored opt-outs such as `!cursed` reduce selection weight when that
feature is requested. Defining and surrounding fragments share the same instance
recording path, with retry rollback restoring level and game totals together.

Living bosses do not lock level exits. Reaching a legal exit permits bypassing the
encounter, but does not grant the boss's kill XP or loot. Normal exploration XP remains unchanged.

## 7. Serialization and validation

[Branch.GetPropertyConditions](Branch.cs) controls which authored properties are
emitted. Update those predicates whenever a branch property or its default changes.
Layout and generator serializers must likewise preserve their authored overrides.
Runtime game-level state is separate from the static branch template.

From the repository root:

```powershell
dotnet test --project test/UnicornHack.Core.Tests --filter-method '*LevelConnectionTest.*' --filter-method '*LevelGeneratorTest.*'
dotnet run --project src/UnicornHack.Editor -- --verify-generated
```

Add fixed-seed tests for your branch's room bounds, reachability, doorway placement,
population limits, reward formulas and boundary destinations. Check retries as well
as successful first attempts: failed generation must restore instance counts and
preserve incoming entrance-room connections.

`--verify-generated` compiles regenerated definitions and compares names, public
members, values and collection membership. Review generated/source differences;
do not treat value or membership changes as formatting noise.

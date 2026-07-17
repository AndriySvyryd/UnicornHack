# UnicornHack - GitHub Copilot Instructions

Guidance for generating code for UnicornHack. If unsure, don't guess: say you don't know or ask. Don't copy a pattern into a different context; evaluate code by implementation and usage, not names. Verify generated code is correct and compilable.

## Project Overview

Traditional turn-based roguelike on .NET: ASP.NET Core Razor Pages web frontend, SignalR multiplayer, procedural level generation, Entity Component System (ECS) with message-driven systems.

Projects: `UnicornHack.Core` (game engine/ECS/logic), `UnicornHack.Web` (Razor Pages + SignalR), `UnicornHack.Editor` (content editing/serialization tools), `UnicornHack.Core.Tests`, `UnicornHack.Core.PerformanceTests`.

Key patterns:
- ECS: entities (players, monsters, items, levels) hold components (data); systems process entities by component combination; systems communicate via messages.
- Messaging: `IMessageQueue`/`SequentialMessageQueue<T>` for ordered processing; messages implement `IMessage`; systems implement `IMessageConsumer<TMessage, TState>`; results control flow.
- State: entities persist via `IRepository`; lifecycle via `GameManager`; turn-based tick timing.

## Game Mechanics

Roguelike dungeon crawler. Attributes: Might, Focus, Perception, Speed. Systems for skills/abilities, equipment, magic (energy-point abilities), and races/traits/feats customization.

Core components: `BeingComponent` (living creatures: HP/EP/attributes), `PlayerComponent` (skill/trait points), `PhysicalComponent` (size/weight/material), `ItemComponent` (type/equipment slots), `AbilityComponent`, `LevelComponent` (levels/terrain), `PositionComponent` (location/movement), `SensorComponent` (vision/sensing), `KnowledgeComponent` (world knowledge).

Key systems: `LivingSystem` (health/energy/attributes), `PlayerSystem` (turn processing/player actions), `AbilityActivationSystem` (casting), `ItemMovingSystem` (inventory/equipment), `TerrainSystem` (geometry/visibility), `KnowledgeSystem` (discovery), `TimeSystem` (turn scheduling).

### Per-Turn Change Set Architecture

When a player acts, the server processes all turns until the player can act again. Instead of one delta covering all accumulated changes, the server collects changes per turn (one entity's action + cascading effects) and sends them as a list of `TurnChangeSet` via the `ReceiveChangeSets` SignalR method. The client applies them sequentially with a configurable delay to animate what happened.

Server flow (`GameStateManager.Turn`):
1. Per turn: clear terrain dicts -> capture visible terrain snapshot -> `TimeSystem.AdvanceSingleTurn()` -> compute visible terrain changes.
2. Per registered `PlayerChangeListener`: if `HasObservableChanges` -> `BuildChangeSet()` -> append to that player's list (a forced emit also happens on the terminating `PlayerTurn` tick to keep the last state aligned).
3. `levelChangeBuilder.Clear()` -> repeat until `TurnResult.PlayerTurn` or `GameOver`.
4. Returns `Dictionary<int, List<TurnChangeSet>>` keyed by player entity id; the hub sends each player their list in one `ReceiveChangeSets` call.

Change detection - under [src/UnicornHack.Web/Hubs/ChangeTracking/](../src/UnicornHack.Web/Hubs/ChangeTracking):
- `LevelChangeBuilder`: shared per-game aggregator. Owns the level-scope builders (`ActorChangeBuilder`, `ItemChangeBuilder`, `ConnectionChangeBuilder`) and a list of `PlayerChangeListener`s. Registered once on the ECS groups; `RegisterPlayerListener` is idempotent.
- `PlayerChangeListener`: per-player. Owns `RaceChangeBuilder`, `AbilityChangeBuilder`, `LogChangeBuilder`. Implements `IEntityChangeListener` on `LevelActors` to capture the player entity's scalar property changes (HP/EP/XP/...) into an internal `_pendingChange: PlayerChange`. `BuildChangeSet` assembles the final `PlayerChange` from level, races, abilities, log, terrain, and scalar bits.
- `ChangeBuilder<TChange>` (abstract): shared state machine for the five entity-scope builders (Actor/Item/Connection/Race/Ability). Handles Added/Modified/Removed tracking and serialization emission. Subclasses override only: which groups to subscribe to, how to filter relevant entities, how to translate property changes onto the DTO, and how to produce full snapshots. See [ChangeBuilder.cs](../src/UnicornHack.Web/Hubs/ChangeTracking/ChangeBuilder.cs) for the state-transition table.
- `TerrainChangeBuilder`: static helper. Diffs the dictionaries `LevelComponent` maintains (`KnownTerrainChanges`, `TerrainChanges`, `WallNeighborsChanges`, `VisibleTerrainChanges`) into a full `LevelMap` or sparse `LevelMapChanges`.
- `LogChangeBuilder`: high-water-mark on log-entry id. Not a `ChangeBuilder` subclass - ordered append-only event streams don't need the state machine.

Invariants enforced by `ChangeBuilder<TChange>`:
- `IChangeWithState.LastState` is assigned once when an entry is created, never mutated after.
- Add-then-Remove within a turn cancels iff `LastState == EntityState.Added` (client never knew the entity).
- `WriteTo` must be called at most once per `Clear` cycle - asserted in debug builds; a duplicate `PlayerChangeListener` registration would violate this, so `RegisterPlayerListener` is idempotent.

Adding a property to a `*Change` DTO: (1) append `[Key(n)]` with the next integer, bump `PropertyCount`; (2) handle it in the builder's `TrackPropertyChanges` (`ItemChangeBuilder`, `ActorChangeBuilder`, `AbilityChangeBuilder`) - or nothing for re-serialize-from-scratch builders (`ConnectionChangeBuilder`, `RaceChangeBuilder`); (3) assign the value in the static `SerializeX` full-snapshot method; (4) add a case to the client-side `Client*.ExpandToCollection` property switch and `Deserialize`.

Initial load (`GetState`): uses `PlayerChangeListener.SerializePlayer` - a full snapshot with `ChangedProperties = null` on every DTO.

`TurnChangeSet`: `Tick` (game tick), `PlayerState` (same format as `GetState`), and `Events` (placeholder for future structured event DTOs).

## Code Style

### General Guidelines

- Follow the [.NET coding guidelines](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) unless overridden below; use the root .editorconfig for ambiguous cases.
- Write clean, maintainable, self-explanatory code; favor readability but keep methods focused. Comment only to explain non-intuitive choices.
- Don't add a UTF-8 BOM unless the file has non-ASCII characters. All types public by default.

### Formatting

- 4-space indentation; braces on new lines for all blocks (including single-line); max line length 140; trim trailing whitespace; final newline at EOF.
- Each declaration on a new line; blank line to separate logical sections.

### C# Specific Guidelines

- File-scoped namespaces; `var` for locals; expression-bodied members where appropriate; collection expressions where possible.
- `is` pattern matching over `as`/null checks; `switch` expressions over statements where appropriate.
- Field-backed properties via the `field` contextual keyword instead of an explicit field; range/index-from-end operators for indexer access.
- Implicit namespaces are on - don't add `using` for already-imported namespaces.
- To verify a file compiles, rebuild the whole project.

### Naming Conventions

- PascalCase: classes, structs, enums, properties, methods, events, namespaces, delegates, public fields, static private fields, constants.
- camelCase: parameters, local variables. `_camelCase`: instance private fields.
- Prefix interfaces with `I`, type parameters with `T`. Use meaningful names and American English spelling.

### Nullability

- Use nullable reference types and proper null-checking; use `?.` and `??` where appropriate.

### Asynchronous Programming

- `Async` suffix; return `Task`/`ValueTask`; avoid `async void` except event handlers; `ConfigureAwait(false)` on awaited calls.

## Performance Considerations

- Mind performance, especially for database operations; avoid unnecessary allocations and GC pressure in game loops.
- On hot paths, prefer more efficient code even if less readable; use object pooling where appropriate.

## ECS Architecture Guidelines

### Entity Management
- Create entities via `GameManager.CreateEntity()`; add/remove components through entity properties or methods; manage relationships via specialized relationship classes.

### Navigation Properties
Relationships maintain navigation properties on components for efficient access to related entities. Always use these instead of `FindEntity(fkId)` or group lookups:

| FK Property | Navigation Property | Returns |
|---|---|---|
| `PositionComponent.LevelId` | `position.LevelEntity` | Level entity |
| `ConnectionComponent.TargetLevelId` | `connection.TargetLevelEntity` | Target level entity |
| `KnowledgeComponent.KnownEntityId` | `knowledge.KnownEntity` | Known entity |
| `ItemComponent.ContainerId` | `item.ContainerEntity` | Container entity |
| `AbilityComponent.OwnerId` | `ability.OwnerEntity` | Owner entity |
| `EffectComponent.AffectedEntityId` | `effect.AffectedEntity` | Affected entity |
| `EffectComponent.SourceAbilityId` | `effect.SourceAbility` | Source ability entity |
| `EffectComponent.SourceEffectId` | `effect.SourceEffect` | Source effect entity |
| `EffectComponent.ContainingAbilityId` | `effect.ContainingAbility` | Containing ability entity |

Collections maintained by relationships:
- `level.Actors/Items/Connections` - `Dictionary<Point, GameEntity>` of actors/items/connections on this level
- `level.KnownActors/KnownItems/KnownConnections` - knowledge entities keyed by position
- `level.IncomingConnections` - connections targeting this level
- `being.Races/Abilities/AppliedEffects/Items/SlottedAbilities` - related entity collections
- `ability.Effects` - effects belonging to this ability
- `position.Knowledge` - back-reference from a level entity to its knowledge entity

### Component Design
- Inherit from `GameComponent`; use `SetWithNotify()` for change notifications; implement `IKeepAliveComponent` to persist; release resources in `Clean()`.

### System Implementation
- Implement `IGameSystem<TMessage>`; register handlers in `GameManager.InitializeSystems()`; use `MessageProcessingResult` to control flow; process entities through entity groups, not direct iteration.

### Message Handling
- Messages are lightweight data carriers; use specific types per event; process in order through the sequential message queue; return appropriate `MessageProcessingResult` values.

## Game-Specific Guidelines

- Entity creation: use the appropriate manager methods, initialize components with proper game references, wire relationships through relationship objects.
- Abilities/effects: abilities define what entities can do; effects modify properties temporarily/permanently via the effect application system.
- Level generation: levels are procedurally generated from map fragments; use the appropriate generators for creatures, items, and terrain.
- Data loading: game data loads through CS script loaders using attribute-based configuration; follow established data-definition patterns.

## Implementation Guidelines

- Write secure-by-default code; don't expose private/sensitive data.
- Make code NativeAOT compatible when possible (avoid dynamic codegen/reflection); otherwise annotate appropriately or throw.
- Follow established patterns, utilities, and error-handling/logging conventions in the codebase.

## Testing Guidelines

- Write unit tests in Core.Tests using `TestHelper`; mock dependencies via established patterns; cover edge cases; use performance tests for critical paths.
- Web.Tests: tests change tracking and serialization without a database; `WebTestHelper` creates an in-memory game, builds levels, and wires up services.

## Web Layer Guidelines

- Follow ASP.NET Core best practices; use SignalR for real-time state; serialize efficiently; handle connection failures gracefully; use proper auth patterns.

## Editor and Tooling

- Use established serialization patterns and existing templates; use `CSClassSerializer` for codegen; maintain backwards compatibility for saved game data.
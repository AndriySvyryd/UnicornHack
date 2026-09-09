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

One player action may advance several internal turns. The server records one change set per turn and sends the resulting batch together; clients apply the sets sequentially so intermediate actions can be animated.

ECS events mark compact change candidates, while persistent projections represent what each client currently knows. Before emission, final world values are compared with those projections so round trips and unchanged candidates produce no transport objects.

Room state, including actor intentions, is observer-independent and built once for players sharing a room. Player scalars, abilities, effects, logs, and events are composed per player.

Nullable integer masks encode payload shape: `null` is a full snapshot, `0` is a deletion where supported, and nonzero values select sparse properties. Per-turn reset clears candidates and prepared data but preserves client-known projections.

Collections and arrays attached to queued responses remain immutable until serialization completes. Mutable or pooled world buffers must be copied before becoming response-owned data.

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
| `EffectComponent.SourceAbilityId` | `effect.SourceAbility` | Source ability entity |
| `EffectComponent.SourceEffectId` | `effect.SourceEffect` | Source effect entity |
| `EffectComponent.ContainingAbilityId` | `effect.ContainingAbility` | Containing ability entity |

Collections maintained by relationships:
- `level.Actors/Items/Connections` - `Dictionary<Point, GameEntity>` of actors/items/connections on this level
- `level.KnownActors/KnownItems/KnownConnections` - knowledge entities keyed by position
- `level.IncomingConnections` - connections targeting this level
- `being.Races/Abilities/Items/SlottedAbilities` - related entity collections
- `ability.Effects` - effects belonging to this ability
- `position.Knowledge` - back-reference from a level entity to its knowledge entity

### Component Design
- Inherit from `GameComponent`; use `SetWithNotify()` for change notifications; implement `IKeepAliveComponent` to persist; release resources in `Clean()`.
- Data only: no business logic, no calls into systems, no cross-entity or index maintenance. Anything reactive belongs in a relationship or system.

### System Implementation
- Implement `IGameSystem<TMessage>`; register handlers in `GameManager.InitializeSystems()`; use `MessageProcessingResult` to control flow; process entities through entity groups, not direct iteration.
- Stateless: hold no data/state fields. Shared indexes, reverse lookups, and relationships live on the `GameManager` (as relationship/index objects), not as fields on the system.

### Message Handling
- Messages are lightweight data carriers; use specific types per event; process in order through the sequential message queue; return appropriate `MessageProcessingResult` values.

## Game-Specific Guidelines

- Entity creation: use the appropriate manager methods, initialize components with proper game references, wire relationships through relationship objects.
- Abilities/effects: abilities define what entities can do; effects modify properties temporarily/permanently via the effect application system.
- Level generation: levels are procedurally generated from map fragments; use the appropriate generators for creatures, items, and terrain.
- Data loading: game data loads through CS script loaders using attribute-based configuration; follow established data-definition patterns.
- Queued messages and pooled objects may outlive entity state. `Clean()` must release references and mutable pooled state.
- Use `entity.ActiveEffects.PropertyEffects` for effects that modify properties and `entity.ActiveEffects.ComponentEffects` for effects that modify components. Do not recover either by scanning effect entities.
- For related-entity property tracking, register `IDependentEntityChangeListener<TEntity>` through `relationship.AddDependentsListener()` so callbacks include the principal; do not emulate relationship semantics with flat-group scans.
- `ManyToManyEntityRelationship<TEntity,TItem>` owns reverse indexes for observable item collections. Set an item's FK before adding it and replace/remove the item instead of mutating its FK in place.
- Procedural generation must use `GameLevel.GenerationRandom`, never `game.Random`.
- Use thread-local pools for reusable mutable objects. Keep immutable write-once memoization caches process-wide (normally `ConcurrentDictionary`); thread-local copies only duplicate compilation/data.
- With EF `ChangingAndChangedNotifications`, owned collection elements must notify changes and owner collections must be observable. Remove entries explicitly/reverse-indexed; do not rely on `List.RemoveAll` notification behavior.
- BitArray-gated MessagePack formatters serialize only set properties. On deserialize, read bit 0 first, reset loop length to `values.Length`, and skip unset indices before reading values; wire position is not the property index.
- Editor round-trip validation is semantic as well as textual. Re-run isolated collection-wrapping-only diffs before accepting them as serializer nondeterminism; never dismiss value, path, or membership changes as formatting noise.

## Implementation Guidelines

- Write secure-by-default code; don't expose private/sensitive data.
- Make code NativeAOT compatible when possible (avoid dynamic codegen/reflection); otherwise annotate appropriately or throw.
- Follow established patterns, utilities, and error-handling/logging conventions in the codebase.

## Testing Guidelines

- Write unit tests in Core.Tests using `TestHelper`; mock dependencies via established patterns; cover edge cases; use performance tests for critical paths.
- Web.Tests: tests change tracking and serialization without a database; `WebTestHelper` creates an in-memory game, builds levels, and wires up services.
- Arrays rented from `ArrayPool` may exceed their logical size. Use the owning count (for terrain, `TileCount`) for copies and iteration.

## Web Layer Guidelines

- Follow ASP.NET Core best practices; use SignalR for real-time state; serialize efficiently; handle connection failures gracefully; use proper auth patterns.

## Editor and Tooling

- Use established serialization patterns and existing templates; use `CSClassSerializer` for codegen; maintain backwards compatibility for saved game data.
- Content lives as C# data files under `UnicornHack.Core/Data/` (creatures, items, abilities, map fragments, ...), loaded by the generators' `Loader`s and generated back out by the `UnicornHack.Editor` console app, which round-trips every type through `CSClassSerializer` into a `New` output folder.
- Ability data is organized under `Data/Abilities/` by broad role: `Skills/`, `Status/`, `Attacks/`, and `Spells/`; cross-cutting templates such as `Innate.cs` remain at the root. `AbilityLoader` owns this mapping and the editor reproduces it.
- Each generation class controls its own serialization via a static `PropertyCSScriptSerializer<T>` built from `GetPropertyConditions<T>()` (a property -> "should emit" predicate, usually "differs from the base/default"). When adding or renaming a serialized property on a generation class, update its `GetPropertyConditions` (and the `Data/` files) so the editor round-trip stays byte-identical.
- Runtime-only changes (systems, message handlers, resolved-property logic) have no serialization surface - only generation classes under `Generation/` and the `Data/` files affect the editor. After changing a generation class, run the editor and diff its `New` output against `Data/` to confirm content round-trips unchanged.
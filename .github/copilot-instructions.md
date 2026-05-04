# UnicornHack - GitHub Copilot Instructions

This document provides guidance for GitHub Copilot when generating code for the UnicornHack project. Follow these guidelines to ensure that generated code aligns with the project's coding standards and architectural principles.

If you are not sure, do not guess, just tell that you don't know or ask clarifying questions. Don't copy code that follows the same pattern in a different context. Don't rely just on names, evaluate the code based on the implementation and usage. Verify that the generated code is correct and compilable.

## Project Overview

UnicornHack is a traditional roguelike game built on .NET, featuring:
- Web-based architecture with ASP.NET Core Razor Pages
- Real-time multiplayer support using SignalR
- Turn-based gameplay with deep character customization
- Procedural level generation
- Entity Component System (ECS) architecture
- Message-driven system architecture

### Architecture Overview

The project follows a modular architecture with clear separation of concerns:

#### Core Projects
- **UnicornHack.Core**: Game engine with ECS architecture, systems, and game logic
- **UnicornHack.Web**: Web frontend using Razor Pages and SignalR for real-time communication
- **UnicornHack.Editor**: Development tools for game content editing and serialization
- **UnicornHack.Core.Tests**: Unit tests for the core game logic
- **UnicornHack.Core.PerformanceTests**: Performance benchmarks

#### Key Architectural Patterns

1. **Entity Component System (ECS)**
   - Entities are game objects (players, monsters, items, levels)
   - Components store data (BeingComponent, PhysicalComponent, ItemComponent, etc.)
   - Systems process entities with specific component combinations
   - Message-driven communication between systems

2. **Messaging System**
   - `IMessageQueue` and `SequentialMessageQueue<T>` for ordered message processing
   - Messages implement `IMessage` interface
   - Systems implement `IMessageConsumer<TMessage, TState>` interface
   - Message processing results control flow

3. **Game State Management**
   - Game entities persist through `IRepository` interface
   - Entity lifecycle managed through `GameManager`
   - Turn-based processing with tick-based timing

## Game Mechanics

### Core Game Concepts

- **Roguelike Gameplay**: Traditional turn-based dungeon crawler
- **Character Attributes**: Might, Focus, Perception, Speed affect combat and abilities
- **Skill System**: Players can learn abilities and improve skills
- **Equipment System**: Items can be equipped, carried, or used
- **Magic System**: Spell-like abilities with energy point costs
- **Races, Traits and Feats**: Character customization through races, traits and feats

### Entity Components

The game uses the following core components:
- `BeingComponent`: Living creatures with stats (HP, EP, attributes)
- `PlayerComponent`: Player-specific data (skill points, trait points)
- `PhysicalComponent`: Physical properties (size, weight, material)
- `ItemComponent`: Item-specific properties (type, equipment slots)
- `AbilityComponent`: Abilities and spells
- `LevelComponent`: Dungeon levels and terrain
- `PositionComponent`: Spatial location and movement
- `SensorComponent`: Vision and sensing capabilities
- `KnowledgeComponent`: What entities know about the world

### Game Systems

Key systems include:
- `LivingSystem`: Health, energy, and attribute management
- `PlayerSystem`: Turn processing and player actions
- `AbilityActivationSystem`: Spell/ability casting
- `ItemMovingSystem`: Inventory and equipment management
- `TerrainSystem`: Level geometry and visibility
- `KnowledgeSystem`: Information tracking and discovery
- `TimeSystem`: Turn scheduling and timing

### Per-Turn Change Set Architecture

When a player performs an action, the server processes all turns until the player can act again. Instead of sending a single delta covering all accumulated changes, the server collects changes **per turn** (one entity's action + cascading effects) and sends them as a list of `TurnChangeSet` objects via the `ReceiveChangeSets` SignalR method. The client applies them sequentially with a configurable delay to animate what happened.

**Server flow** (`GameStateManager.Turn`):
1. For each turn: clear terrain dicts → capture visible terrain snapshot → `TimeSystem.AdvanceSingleTurn()` → compute visible terrain changes
2. For each registered `PlayerChangeListener`: if `HasObservableChanges` → `BuildChangeSet()` → append to that player's list (a forced emit also happens on the terminating `PlayerTurn` tick to keep the last state aligned)
3. `levelChangeBuilder.Clear()` → repeat until `TurnResult.PlayerTurn` or `GameOver`
4. Returns `Dictionary<int, List<TurnChangeSet>>` keyed by player entity id; the hub sends each player their list in one `ReceiveChangeSets` call

**Change detection** — organized under [src/UnicornHack.Web/Hubs/ChangeTracking/](src/UnicornHack.Web/Hubs/ChangeTracking):
- `LevelChangeBuilder`: shared per-game aggregator. Owns the level-scope builders (`ActorChangeBuilder`, `ItemChangeBuilder`, `ConnectionChangeBuilder`) and a list of `PlayerChangeListener`s. Registered once on the ECS groups; `RegisterPlayerListener` is idempotent.
- `PlayerChangeListener`: per-player. Owns `RaceChangeBuilder`, `AbilityChangeBuilder`, `LogChangeBuilder`. Implements `IEntityChangeListener` on `LevelActors` to capture the player entity's scalar property changes (HP/EP/XP/…) into an internal `_pendingChange: PlayerChange`. `BuildChangeSet` assembles the final `PlayerChange` by combining level, races, abilities, log, terrain, and scalar bits.
- `ChangeBuilder<TChange>` (abstract): shared state machine for the five entity-scope builders (Actor/Item/Connection/Race/Ability). Encapsulates Added/Modified/Removed tracking and serialization emission. Subclasses override only: which groups to subscribe to, how to filter relevant entities, how to translate property changes onto the DTO, and how to produce full snapshots. See [ChangeBuilder.cs](src/UnicornHack.Web/Hubs/ChangeTracking/ChangeBuilder.cs) for the state-transition table.
- `TerrainChangeBuilder`: static helper. Diffs the dictionaries `LevelComponent` maintains (`KnownTerrainChanges`, `TerrainChanges`, `WallNeighborsChanges`, `VisibleTerrainChanges`) into a full `LevelMap` or sparse `LevelMapChanges`.
- `LogChangeBuilder`: high-water-mark on log-entry id. Intentionally not a `ChangeBuilder` subclass — ordered append-only event streams don't need the state machine.

**Invariants** enforced by `ChangeBuilder<TChange>`:
- `IChangeWithState.LastState` is assigned once when an entry is created and never mutated afterward.
- Add-then-Remove within a turn cancels iff `LastState == EntityState.Added` (client never knew the entity).
- `WriteTo` must be called at most once per `Clear` cycle — asserted in debug builds. A duplicate `PlayerChangeListener` registration would violate this; `LevelChangeBuilder.RegisterPlayerListener` is idempotent.

**Adding a property to a `*Change` DTO**: (1) append `[Key(n)]` with the next integer, bump `PropertyCount`, (2) handle the property in the builder's `TrackPropertyChanges` (`ItemChangeBuilder`, `ActorChangeBuilder`, `AbilityChangeBuilder`) — or nothing for re-serialize-from-scratch builders (`ConnectionChangeBuilder`, `RaceChangeBuilder`), (3) assign the value in the static `SerializeX` full-snapshot method, (4) add a case to the client-side `Client*.ExpandToCollection` property switch and `Deserialize`.

**Initial load** (`GetState`): uses `PlayerChangeListener.SerializePlayer` — a full snapshot with `ChangedProperties = null` on every DTO.

**`TurnChangeSet`**: Contains `Tick` (game tick), `PlayerState` (same serialized format as `GetState`), and `Events` (placeholder for future structured event DTOs).

## Code Style

### General Guidelines

- Follow the [.NET coding guidelines](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) unless explicitly overridden below
- Use the rules defined in the .editorconfig file in the root of the repository for any ambiguous cases
- Write code that is clean, maintainable, and easy to understand
- Favor readability over brevity, but keep methods focused and concise
- Only add comments rarely to explain why a non-intuitive solution was used. The code should be self-explanatory otherwise
- Don't add the UTF-8 BOM to files unless they have non-ASCII characters
- All types should be public by default

### Formatting

- Use spaces for indentation (4 spaces)
- Use braces for all blocks including single-line blocks
- Place braces on new lines
- Limit line length to 140 characters
- Trim trailing whitespace
- All declarations must begin on a new line
- Use a single blank line to separate logical sections of code when appropriate
- Insert a final newline at the end of files

### C# Specific Guidelines

- File scoped namespace declarations
- Use `var` for local variables
- Use expression-bodied members where appropriate
- Prefer using collection expressions when possible
- Use `is` pattern matching instead of `as` and null checks
- Prefer `switch` expressions over `switch` statements when appropriate
- Prefer field-backed property declarations using field contextual keyword instead of an explicit field.
- Prefer range and index from end operators for indexer access
- The projects use implicit namespaces, so do not add `using` directives for namespaces that are already imported by the project
- When verifying that a file doesn't produce compiler errors rebuild the whole project

### Naming Conventions

- Use PascalCase for:
  - Classes, structs, enums, properties, methods, events, namespaces, delegates
  - Public fields
  - Static private fields
  - Constants
- Use camelCase for:
  - Parameters
  - Local variables
- Use `_camelCase` for instance private fields
- Prefix interfaces with `I`
- Prefix type parameters with `T`
- Use meaningful and descriptive names

### Nullability

- Use nullable reference types
- Use proper null-checking patterns
- Use the null-conditional operator (`?.`) and null-coalescing operator (`??`) when appropriate

### Asynchronous Programming

- Use the `Async` suffix for asynchronous methods
- Return `Task` or `ValueTask` from asynchronous methods
- Avoid async void methods except for event handlers
- Call `ConfigureAwait(false)` on awaited calls to avoid deadlocks

## Performance Considerations

- Be mindful of performance implications, especially for database operations
- Avoid unnecessary allocations
- Consider using more efficient code that is expected to be on the hot path, even if it is less readable
- Use object pooling where appropriate
- Be aware of garbage collection pressure in game loops

## ECS Architecture Guidelines

### Entity Management
- Entities are created through `GameManager.CreateEntity()`
- Components are added/removed through entity properties or methods
- Entity relationships are managed through specialized relationship classes

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
- `level.Actors/Items/Connections` — `Dictionary<Point, GameEntity>` of actors/items/connections on this level
- `level.KnownActors/KnownItems/KnownConnections` — knowledge entities keyed by position
- `level.IncomingConnections` — connections targeting this level
- `being.Races/Abilities/AppliedEffects/Items/SlottedAbilities` — related entity collections
- `ability.Effects` — effects belonging to this ability
- `position.Knowledge` — back-reference from a level entity to its knowledge entity

### Component Design
- Components inherit from `GameComponent` for game-specific functionality
- Use property change notifications via `SetWithNotify()` for data binding
- Implement `IKeepAliveComponent` for components that should persist
- Clean up resources in the `Clean()` method

### System Implementation
- Systems implement `IGameSystem<TMessage>` interface
- Register message handlers in `GameManager.InitializeSystems()`
- Use `MessageProcessingResult` to control message flow
- Process entities through entity groups, not direct iteration

### Message Handling
- Messages are lightweight data carriers
- Use specific message types for different events
- Process messages in order through the sequential message queue
- Return appropriate `MessageProcessingResult` values

## Game-Specific Guidelines

### Entity Creation Patterns
- Use the appropriate manager methods for creating game entities
- Initialize components with proper game references
- Set up entity relationships through the appropriate relationship objects

### Ability and Effect System
- Abilities define what entities can do
- Effects modify entity properties temporarily or permanently
- Use the effect application system for consistent property modification

### Level Generation
- Levels are procedurally generated using map fragments
- Fragment composition creates complex level layouts
- Use the appropriate generators for creatures, items, and terrain

### Data Loading
- Game data is loaded through CS script loaders
- Use attribute-based configuration for data objects
- Follow the established patterns for data definition files

## Implementation Guidelines

- Write code that is secure by default. Avoid exposing potentially private or sensitive data
- Make code NativeAOT compatible when possible. This means avoiding dynamic code generation, reflection, and other features that are not compatible with NativeAOT. If not possible, mark the code with an appropriate annotation or throw an exception
- Follow established patterns for similar functionality in the codebase
- Use the existing utility classes and helper methods where appropriate
- Maintain consistency with the existing error handling and logging patterns

## Testing Guidelines

- Write unit tests for new functionality in the Core.Tests project
- Use the `TestHelper` class for common test setup
- Mock dependencies appropriately using the established patterns
- Test edge cases and error conditions
- Use performance tests for critical path optimizations

### Web.Tests Project
- Tests change tracking and serialization correctness without a database
- `WebTestHelper` creates an in-memory game, builds levels, and wires up the necessary services

## Web Layer Guidelines

- Razor Pages should follow ASP.NET Core best practices
- Use SignalR for real-time game state updates
- Serialize data efficiently for network transmission
- Handle connection failures gracefully
- Use proper authentication and authorization patterns

## Editor and Tooling

- Editor tools should use the established serialization patterns
- Code generation tools should follow the existing templates
- Use the `CSClassSerializer` for automated code generation
- Maintain backwards compatibility for saved game data
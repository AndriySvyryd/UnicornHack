using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;

namespace UnicornHack.Data;

public class GameDbContext : DbContext, IRepository
{
    private Func<GameDbContext, int, IEnumerable<int>>? _getRoomIds;

    // ReSharper disable once SuggestBaseTypeForParameter
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    public GameManager Manager
    {
        get;
        set;
    } = null!;

    public RoomChangeBuilder? RoomChangeBuilder
    {
        get;
        set;
    }

    public DbSet<Game> Games
    {
        get;
        set;
    } = null!;

    public DbSet<GameBranch> Branches
    {
        get;
        set;
    } = null!;

    public DbSet<GameLevel> Levels
    {
        get;
        set;
    } = null!;

    public DbSet<LogEntry> LogEntries
    {
        get;
        set;
    } = null!;

    public DbSet<PlayerCommand> PlayerCommands
    {
        get;
        set;
    } = null!;

    public DbSet<GameEntity> Entities
    {
        get;
        set;
    } = null!;

    public DbSet<AbilityComponent> AbilityComponents
    {
        get;
        set;
    } = null!;

    public DbSet<AIComponent> AIComponents
    {
        get;
        set;
    } = null!;

    public DbSet<PlayerComponent> PlayerComponents
    {
        get;
        set;
    } = null!;

    public DbSet<BeingComponent> BeingComponents
    {
        get;
        set;
    } = null!;

    public DbSet<PhysicalComponent> PhysicalComponents
    {
        get;
        set;
    } = null!;

    public DbSet<RaceComponent> RaceComponents
    {
        get;
        set;
    } = null!;

    public DbSet<EffectComponent> EffectComponents
    {
        get;
        set;
    } = null!;

    public DbSet<ItemComponent> ItemComponents
    {
        get;
        set;
    } = null!;

    public DbSet<KnowledgeComponent> KnowledgeComponents
    {
        get;
        set;
    } = null!;

    public DbSet<ConnectionComponent> ConnectionComponents
    {
        get;
        set;
    } = null!;

    public DbSet<RoomComponent> RoomComponents
    {
        get;
        set;
    } = null!;

    public DbSet<PositionComponent> PositionComponents
    {
        get;
        set;
    } = null!;

    public DbSet<SensorComponent> SensorComponents
    {
        get;
        set;
    } = null!;

    private IQueryable<GameEntity> AggregateEntities
        => Entities
            .Include(e => e.Room).ThenInclude(r => r!.Level).ThenInclude(l => l!.Branch)
            .Include(e => e.Position)
            .Include(e => e.Connection)
            .Include(e => e.Being)
            .Include(e => e.Player)
            .Include(e => e.AI)
            .Include(e => e.Sensor)
            .Include(e => e.Physical)
            .Include(e => e.Item)
            .Include(e => e.Knowledge)
            .Include(e => e.Race)
            .Include(e => e.Ability)
            .Include(e => e.Effect);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: Use pre-convention model configuration to ignore all collection types

        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty);

        modelBuilder.Entity<Game>(eb =>
        {
            eb.Ignore(g => g.Services);
            eb.Ignore(g => g.Manager);
            eb.Ignore(g => g.Repository);

            eb.Property(g => g.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();
            eb.OwnsOne(g => g.Random);
            eb.Navigation(g => g.Random).IsRequired();
            eb.OwnsOne(g => g.Options, ob =>
            {
                ob.Ignore(o => o.RevealLevels);
                ob.Ignore(o => o.RevealRooms);
                ob.Ignore(o => o.DisableSessionCache);
            });
            eb.Navigation(g => g.Options).IsRequired();
            eb.OwnsOne(g => g.GenerationStats, ConfigureGenerationStats);
            eb.Navigation(g => g.GenerationStats).IsRequired();
            eb.HasOne(g => g.ActingPlayer)
                .WithOne()
                .HasForeignKey<Game>(l => new { l.Id, l.ActingPlayerId })
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany(g => g.Branches)
                .WithOne(l => l.Game)
                .HasForeignKey(l => l.GameId);
            eb.HasMany<GameLevel>()
                .WithOne(l => l.Game)
                .HasForeignKey(l => l.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<LogEntry>()
                .WithOne()
                .HasForeignKey(l => l.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<PlayerCommand>()
                .WithOne()
                .HasForeignKey(l => l.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<GameEntity>()
                .WithOne()
                .HasForeignKey(a => a.GameId);
            eb.HasMany<AbilityComponent>()
                .WithOne()
                .HasForeignKey(a => a.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<AIComponent>()
                .WithOne()
                .HasForeignKey(s => s.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<PlayerComponent>()
                .WithOne()
                .HasForeignKey(a => a.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<BeingComponent>()
                .WithOne()
                .HasForeignKey(a => a.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<PhysicalComponent>()
                .WithOne()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<RaceComponent>()
                .WithOne()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<EffectComponent>()
                .WithOne()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<ItemComponent>()
                .WithOne()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<KnowledgeComponent>()
                .WithOne()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<ConnectionComponent>()
                .WithOne()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<RoomComponent>()
                .WithOne()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<PositionComponent>()
                .WithOne()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            eb.HasMany<SensorComponent>()
                .WithOne()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GameBranch>(eb =>
        {
            eb.HasKey(b => new { b.GameId, b.Name });
            eb.HasMany(b => b.Levels)
                .WithOne(l => l.Branch)
                .HasForeignKey(l => new { l.GameId, l.BranchName })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GameLevel>(eb =>
        {
            eb.HasKey(l => new { l.GameId, l.BranchName, l.Depth });
            eb.OwnsOne(l => l.GenerationRandom);
            eb.Navigation(l => l.GenerationRandom).IsRequired();
            eb.OwnsOne(l => l.GenerationStats, ConfigureLevelGenerationStats);
            eb.Navigation(l => l.GenerationStats).IsRequired();
            eb.HasMany(l => l.Rooms)
                .WithOne(r => r.Level)
                .HasForeignKey(r => new { r.GameId, r.BranchName, r.Depth })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LogEntry>(eb =>
        {
            eb.HasKey(e => new { e.GameId, e.Id });
            eb.Property(l => l.Message).IsRequired();
            eb.HasOne<PlayerComponent>()
                .WithMany(pc => pc.LogEntries)
                .HasForeignKey(l => new { l.GameId, l.PlayerId });
        });

        modelBuilder.Entity<PlayerCommand>(eb =>
        {
            eb.HasKey(e => new { e.GameId, e.Id });
            eb.HasOne<PlayerComponent>()
                .WithMany(pc => pc.CommandHistory)
                .HasForeignKey(l => new { l.GameId, l.PlayerId });
        });

        modelBuilder.Entity<GameEntity>(eb =>
        {
            eb.Ignore(e => e.Manager);
            eb.Property("_referenceCount");
            eb.HasKey(e => new { e.GameId, e.Id });
            eb.HasOne(e => e.Ability).WithOne(c => c.Entity)
                .HasForeignKey<AbilityComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.AI).WithOne(c => c.Entity).HasForeignKey<AIComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Being).WithOne(c => c.Entity)
                .HasForeignKey<BeingComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Connection).WithOne(c => c.Entity)
                .HasForeignKey<ConnectionComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Effect).WithOne(c => c.Entity)
                .HasForeignKey<EffectComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Item).WithOne(c => c.Entity)
                .HasForeignKey<ItemComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Knowledge).WithOne(c => c.Entity)
                .HasForeignKey<KnowledgeComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Room).WithOne(c => c.Entity)
                .HasForeignKey<RoomComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Physical).WithOne(c => c.Entity)
                .HasForeignKey<PhysicalComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Position).WithOne(c => c.Entity)
                .HasForeignKey<PositionComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Player).WithOne(c => c.Entity)
                .HasForeignKey<PlayerComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Race).WithOne(c => c.Entity)
                .HasForeignKey<RaceComponent>(c => new { c.GameId, c.EntityId });
            eb.HasOne(e => e.Sensor).WithOne(c => c.Entity)
                .HasForeignKey<SensorComponent>(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<AbilityComponent>(eb =>
        {
            eb.Ignore(c => c.OwnerEntity);
            eb.Ignore(c => c.Template);
            eb.Ignore(c => c.AccuracyFunction);
            eb.Ignore(c => c.DelayFunction);
            eb.Ignore(c => c.Effects);
            // TODO: Consider whether this needs to be persisted
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<AIComponent>(eb =>
        {
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<PlayerComponent>(eb =>
        {
            eb.Property("_tracked");
            eb.HasIndex(p => p.ProperName).IsUnique();
            eb.HasKey(p => new { p.GameId, p.EntityId });
        });

        modelBuilder.Entity<BeingComponent>(eb =>
        {
            eb.Ignore(c => c.IsAlive);
            eb.Ignore(c => c.Abilities);
            eb.Ignore(c => c.AppliedEffects);
            eb.Ignore(c => c.SlottedAbilities);
            eb.Ignore(c => c.Races);
            eb.Ignore(c => c.Items);
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<PhysicalComponent>(eb =>
        {
            eb.Ignore(c => c.Abilities);
            eb.Ignore(c => c.AppliedEffects);
            eb.Ignore(c => c.Items);
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<RaceComponent>(eb =>
        {
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<EffectComponent>(eb =>
        {
            eb.Ignore(c => c.AffectedEntity);
            eb.Ignore(c => c.SourceEffect);
            eb.Ignore(c => c.SourceAbility);
            eb.Ignore(c => c.ContainingAbility);
            eb.Ignore(c => c.AmountFunction);
            eb.Ignore(c => c.SecondaryAmountFunction);
            eb.Ignore(c => c.DurationAmountFunction);
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<ItemComponent>(eb =>
        {
            eb.Ignore(c => c.ContainerEntity);
            eb.Ignore(c => c.Abilities);
            eb.Ignore(c => c.AppliedEffects);
            eb.Ignore(c => c.Items);
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<KnowledgeComponent>(eb =>
        {
            eb.Ignore(c => c.KnownEntity);
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<ConnectionComponent>(eb =>
        {
            eb.Ignore(c => c.TargetCell);
            eb.Ignore(c => c.TargetRoomEntity);
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<RoomComponent>(eb =>
        {
            eb.Ignore(l => l.Actors);
            eb.Ignore(l => l.Items);
            eb.Ignore(l => l.Connections);
            eb.Ignore(l => l.IncomingConnections);
            eb.Ignore(l => l.KnownActors);
            eb.Ignore(l => l.KnownItems);
            eb.Ignore(l => l.KnownConnections);
            eb.Ignore(l => l.IndexToPoint);
            eb.Ignore(l => l.PointToIndex);
            eb.Ignore(l => l.TerrainChanges);
            eb.Ignore(l => l.KnownTerrainChanges);
            eb.Ignore(l => l.NeighborChanges);
            eb.Ignore(l => l.VisibleTerrainSnapshot);
            eb.Ignore(l => l.VisibleTerrainChanges);
            eb.Ignore(l => l.KnownNeighborsChanged);
            eb.Ignore(l => l.BoundPlaceableFeatures);
            eb.Ignore(l => l.PathFinder);
            eb.Ignore(l => l.VisibilityCalculator);
            eb.Property("_tracked");

            eb.Property(l => l.Terrain)
                .HasConversion(MapFeatureArrayConverter, MapFeatureArrayComparer);
            eb.Property(l => l.KnownTerrain)
                .HasConversion(MapFeatureArrayConverter, MapFeatureArrayComparer);
            eb.Property(l => l.Neighbors)
                .HasConversion(DirectionFlagsArrayConverter, DirectionFlagsArrayComparer);
            eb.Property(l => l.KnownNeighbors)
                .HasConversion(DirectionFlagsArrayConverter, DirectionFlagsArrayComparer);

            eb.HasKey(l => new { l.GameId, l.EntityId });
        });

        modelBuilder.Entity<PositionComponent>(eb =>
        {
            eb.Ignore(c => c.Cell);
            eb.Ignore(c => c.RoomEntity);
            eb.Ignore(c => c.Knowledge);
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<SensorComponent>(eb =>
        {
            eb.Ignore(c => c.VisibleTerrain);
            eb.Ignore(c => c.VisibleTerrainIsCurrent);
            eb.Ignore(c => c.Abilities);
            eb.Ignore(c => c.AppliedEffects);
            eb.Ignore(c => c.Items);
            eb.Property("_tracked");
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });
    }

    public Game LoadGame(int gameId)
    {
        // For each player: emit the player's current room id, plus - by walking
        // player -> position (player's room id) -> player's room (BranchName, Depth)
        // -> every room on that level -> that room's connections - each connection's
        // target room id. Union deduplicates.
        _getRoomIds ??= EF.CompileQuery((GameDbContext context, int id) =>
            context.PlayerComponents
                .Where(playerComponent => playerComponent.GameId == id)
                .Join(context.PositionComponents,
                    playerComponent => new { playerComponent.GameId, playerComponent.EntityId },
                    positionComponent => new { positionComponent.GameId, positionComponent.EntityId },
                    (_, positionComponent) => positionComponent.RoomId)
                .Union(context.PlayerComponents
                    .Where(playerComponent => playerComponent.GameId == id)
                    .Join(context.PositionComponents,
                        playerComponent => new { playerComponent.GameId, playerComponent.EntityId },
                        positionComponent => new { positionComponent.GameId, positionComponent.EntityId },
                        (_, positionComponent) => positionComponent.RoomId)
                    .Join(context.RoomComponents,
                        roomId => new { GameId = id, EntityId = roomId },
                        roomComponent => new { roomComponent.GameId, roomComponent.EntityId },
                        (_, playerRoom) => new { playerRoom.BranchName, playerRoom.Depth })
                    .Join(context.RoomComponents,
                        level => new { GameId = id, level.BranchName, level.Depth },
                        roomComponent => new { roomComponent.GameId, roomComponent.BranchName, roomComponent.Depth },
                        (_, levelRoom) => levelRoom.EntityId)
                    .Join(context.PositionComponents,
                        levelRoomId => new { GameId = id, RoomId = levelRoomId },
                        positionComponent => new { positionComponent.GameId, positionComponent.RoomId },
                        (_, positionComponent) => new { positionComponent.GameId, positionComponent.EntityId })
                    .Join(context.ConnectionComponents,
                        x => new { x.GameId, x.EntityId },
                        connectionComponent => new { connectionComponent.GameId, connectionComponent.EntityId },
                        (_, connectionComponent) => connectionComponent.TargetRoomId)));

        LoadRooms(_getRoomIds(this, gameId).ToList());

        // Load the 10 most recent log entries per player.
        var playerIds = PlayerComponents.Local.Select(p => p.EntityId).ToList();

        LogEntries
            .Where(e => e.GameId == gameId
                && playerIds.Contains(e.PlayerId)
                && LogEntries
                    .Where(e2 => e2.GameId == gameId && e2.PlayerId == e.PlayerId)
                    .OrderByDescending(e2 => e2.Tick)
                    .ThenByDescending(e2 => e2.Id)
                    .Take(10)
                    .Contains(e))
            .Load();

        return Find<Game>(gameId)!;
    }

    public void LoadRooms(IReadOnlyList<int> roomIds)
    {
        Manager.IsLoading = true;

        if (roomIds.Count == 0)
        {
            Manager.IsLoading = false;
            return;
        }

        // 1. Entities that ARE one of the requested rooms or are positioned in one.
        //    Split into two Concat branches so each becomes a single-predicate query
        //    against the roomIds parameter - see the nestedMix comment below for why.
        //    Distinct is required because a room entity also has itself in Position.
        var roomResidentIds = Entities
            .Where(e => roomIds.Contains(e.Id))
            .Select(e => e.Id)
            .Concat(Entities
                .Where(e => roomIds.Contains(e.Position!.RoomId))
                .Select(e => e.Id))
            .Distinct()
            .ToList();

        // 2. Items at containment depths 1 and 2 (via EXISTS into ItemComponents),
        //    plus primary and secondary natural weapons of beings in the room set.
        //    All four projections share the same roomResidentIds list parameter.
        //    TODO: Query additional nested items
        List<int> itemAndWeaponIds;
        if (roomResidentIds.Count == 0)
        {
            itemAndWeaponIds = [];
        }
        else
        {
            // Split the depth-1 and depth-2 item lookups into separate Concat
            // branches so each is a single-predicate query against roomResidentIds
            // (see nestedMix comment below). The downstream affectableIds.Distinct()
            // removes any item that matches both branches.
            itemAndWeaponIds = ItemComponents
                .Where(ic => ic.ContainerId.HasValue && roomResidentIds.Contains(ic.ContainerId.Value))
                .Select(ic => ic.EntityId)
                .Concat(ItemComponents
                    .Where(ic => ic.ContainerId.HasValue
                                 && ItemComponents.Any(parent =>
                                     parent.EntityId == ic.ContainerId.Value
                                     && parent.ContainerId.HasValue
                                     && roomResidentIds.Contains(parent.ContainerId.Value)))
                    .Select(ic => ic.EntityId))
                .Concat(BeingComponents
                    .Where(bc => roomResidentIds.Contains(bc.EntityId) && bc.PrimaryNaturalWeaponId.HasValue)
                    .Select(bc => bc.PrimaryNaturalWeaponId!.Value))
                .Concat(BeingComponents
                    .Where(bc => roomResidentIds.Contains(bc.EntityId) && bc.SecondaryNaturalWeaponId.HasValue)
                    .Select(bc => bc.SecondaryNaturalWeaponId!.Value))
                .ToList();
        }

        var affectableIds = roomResidentIds
            .Concat(itemAndWeaponIds)
            .Distinct()
            .ToList();

        // 3. Top-level abilities AND top-level effects in one round-trip.
        //    - Abilities owned by an affectable (Kind = 0)
        //    - Effects either directly affecting an affectable, OR contained
        //      in an ability that itself is owned by an affectable (Kind = 1).
        //      The second branch is expressed as EXISTS against AbilityComponents
        //      so we don't need a materialized topAbilityIds list yet.
        var topMix = affectableIds.Count == 0
            ? []
            : AbilityComponents
                .Where(ac => ac.OwnerId.HasValue && affectableIds.Contains(ac.OwnerId.Value))
                .Select(ac => new { Id = ac.EntityId, IsEffect = false })
                .Concat(EffectComponents
                    .Where(ec => ec.AffectedEntityId.HasValue && affectableIds.Contains(ec.AffectedEntityId.Value))
                    .Select(ec => new { Id = ec.EntityId, IsEffect = true }))
                .Concat(EffectComponents
                    .Where(ec => ec.ContainingAbilityId.HasValue
                                 && AbilityComponents.Any(ac =>
                                     ac.EntityId == ec.ContainingAbilityId.Value
                                     && ac.OwnerId.HasValue
                                     && affectableIds.Contains(ac.OwnerId.Value)))
                    .Select(ec => new { Id = ec.EntityId, IsEffect = true }))
                .ToList();

        var topAbilityIds = topMix.Where(x => !x.IsEffect).Select(x => x.Id).ToList();
        var topEffectIds = topMix.Where(x => x.IsEffect).Select(x => x.Id).Distinct().ToList();

        // 4. Nested abilities and effects (one more graph level) in one round-trip.
        //    Each branch of the Concat is a single-predicate query against the
        //    topEffectIds list - this is deliberate. A single Where(ec => A || B || C)
        //    where each clause references the same OPENJSON parameter forces SQL
        //    Server into nested loops that rebuild OPENJSON per outer row (the
        //    previous plan rebound it ~4500 times and ran in 6+ seconds). Splitting
        //    into UNION ALL lets each branch use a hash semi join that builds
        //    OPENJSON once and hash-probes the source table. Duplicate effect ids
        //    across branches are removed by the client-side Distinct() below.
        //    - Abilities owned by a top-level effect (Kind = 0)
        //    - Effects directly affecting a top-level effect (Kind = 1)
        //    - Effects directly affecting a top-level ability (Kind = 1)
        //    - Effects contained in a top-level effect (Kind = 1)
        //    - Effects contained in a nested ability - expressed as EXISTS so we
        //      don't need a materialized nestedAbilityIds list yet (Kind = 1).
        var nestedMix = topEffectIds.Count == 0 && topAbilityIds.Count == 0
            ? []
            : AbilityComponents
                .Where(ac => ac.OwnerId.HasValue && topEffectIds.Contains(ac.OwnerId.Value))
                .Select(ac => new { Id = ac.EntityId, IsEffect = false })
                .Concat(EffectComponents
                    .Where(ec => ec.AffectedEntityId.HasValue && topEffectIds.Contains(ec.AffectedEntityId.Value))
                    .Select(ec => new { Id = ec.EntityId, IsEffect = true }))
                .Concat(EffectComponents
                    .Where(ec => ec.AffectedEntityId.HasValue && topAbilityIds.Contains(ec.AffectedEntityId.Value))
                    .Select(ec => new { Id = ec.EntityId, IsEffect = true }))
                .Concat(EffectComponents
                    .Where(ec => ec.ContainingAbilityId.HasValue && topEffectIds.Contains(ec.ContainingAbilityId.Value))
                    .Select(ec => new { Id = ec.EntityId, IsEffect = true }))
                .Concat(EffectComponents
                    .Where(ec => ec.ContainingAbilityId.HasValue
                                 && AbilityComponents.Any(ac =>
                                     ac.EntityId == ec.ContainingAbilityId.Value
                                     && ac.OwnerId.HasValue
                                     && topEffectIds.Contains(ac.OwnerId.Value)))
                    .Select(ec => new { Id = ec.EntityId, IsEffect = true }))
                .ToList();

        var nestedAbilityIds = nestedMix.Where(x => !x.IsEffect).Select(x => x.Id).ToList();
        var nestedEffectIds = nestedMix.Where(x => x.IsEffect).Select(x => x.Id).Distinct().ToList();

        // 6. Effects directly on a nested ability. nestedAbilityIds wasn't
        //    materialized at step 5, so this is a separate round-trip.
        var abilityEffectIds = nestedAbilityIds.Count == 0
            ? []
            : EffectComponents
                .Where(ec => ec.AffectedEntityId.HasValue && nestedAbilityIds.Contains(ec.AffectedEntityId.Value))
                .Select(ec => ec.EntityId)
                .ToList();

        // 5. Load every reachable entity in a single query together with all of
        //    its components. Including every component navigation is safe even
        //    for entities that wouldn't normally carry them - the LEFT JOINs
        //    simply return nulls - and it guards against unexpected component
        //    combinations.
        var allEntityIds = affectableIds
            .Concat(topAbilityIds)
            .Concat(topEffectIds)
            .Concat(nestedAbilityIds)
            .Concat(nestedEffectIds)
            .Concat(abilityEffectIds)
            .Distinct()
            .ToList();

        if (allEntityIds.Count > 0)
        {
            Entities.Where(e => allEntityIds.Contains(e.Id))
                .Include(e => e.Room!).ThenInclude(l => l.Level).ThenInclude(l => l!.Branch)
                .Include(e => e.Position)
                .Include(e => e.Connection)
                .Include(e => e.Being)
                .Include(e => e.Player)
                .Include(e => e.AI)
                .Include(e => e.Sensor)
                .Include(e => e.Physical)
                .Include(e => e.Item)
                .Include(e => e.Knowledge)
                .Include(e => e.Ability)
                .Include(e => e.Effect)
                .Include(e => e.Race)
                .Load();
        }

        Manager.IsLoading = false;

        foreach (var level in RoomComponents.Local)
        {
            level.EnsureInitialized();
        }
    }

    void IRepository.Add<T>(T entity)
        => Entry(entity).State = EntityState.Added;

    void IRepository.Remove(object entity)
        => Entry(entity).State = EntityState.Deleted;

    T? IRepository.Find<T>(params object[] keyValues) where T : class
    {
        var entity = Find<T>(keyValues);
        if (entity == null)
        {
            return null;
        }

        if (Entry(entity).State == EntityState.Deleted)
        {
            SaveChanges();
            return null;
        }

        return entity;
    }

    public void RemoveTracked(object entity)
    {
        var entry = Entry(entity);
        switch (entry.State)
        {
            case EntityState.Added:
                entry.State = EntityState.Detached;
                break;
            case EntityState.Unchanged:
            case EntityState.Modified:
                entry.State = EntityState.Deleted;
                break;
        }
    }

    public void Clean(Game game)
    {
        foreach (var component in RoomComponents.Where(b => b.GameId == game.Id))
        {
            RoomComponents.Remove(component);
        }

        foreach (var branch in Branches.Where(b => b.GameId == game.Id))
        {
            Branches.Remove(branch);
        }

        foreach (var level in Levels.Where(b => b.GameId == game.Id))
        {
            Levels.Remove(level);
        }

        foreach (var logEntry in LogEntries.Where(e => e.GameId == game.Id))
        {
            LogEntries.Remove(logEntry);
        }

        foreach (var playerCommand in PlayerCommands.Where(b => b.GameId == game.Id))
        {
            PlayerCommands.Remove(playerCommand);
        }

        foreach (var abilityComponent in AbilityComponents.Where(b => b.GameId == game.Id))
        {
            AbilityComponents.Remove(abilityComponent);
        }

        foreach (var aiComponent in AIComponents.Where(b => b.GameId == game.Id))
        {
            AIComponents.Remove(aiComponent);
        }

        foreach (var playerComponent in PlayerComponents.Where(b => b.GameId == game.Id))
        {
            PlayerComponents.Remove(playerComponent);
        }

        foreach (var beingComponent in BeingComponents.Where(b => b.GameId == game.Id))
        {
            BeingComponents.Remove(beingComponent);
        }

        foreach (var physicalComponent in PhysicalComponents.Where(b => b.GameId == game.Id))
        {
            PhysicalComponents.Remove(physicalComponent);
        }

        foreach (var raceComponent in RaceComponents.Where(b => b.GameId == game.Id))
        {
            RaceComponents.Remove(raceComponent);
        }

        foreach (var effectComponent in EffectComponents.Where(b => b.GameId == game.Id))
        {
            EffectComponents.Remove(effectComponent);
        }

        foreach (var itemComponent in ItemComponents.Where(b => b.GameId == game.Id))
        {
            ItemComponents.Remove(itemComponent);
        }

        foreach (var knowledgeComponent in KnowledgeComponents.Where(b => b.GameId == game.Id))
        {
            KnowledgeComponents.Remove(knowledgeComponent);
        }

        foreach (var connectionComponent in ConnectionComponents.Where(b => b.GameId == game.Id))
        {
            ConnectionComponents.Remove(connectionComponent);
        }

        foreach (var positionComponent in PositionComponents.Where(b => b.GameId == game.Id))
        {
            PositionComponents.Remove(positionComponent);
        }

        foreach (var sensorComponent in SensorComponents.Where(b => b.GameId == game.Id))
        {
            SensorComponents.Remove(sensorComponent);
        }

        game.ActingPlayerId = null;

        SaveChanges();

        Games.Remove(game);

        SaveChanges();

        Debug.Assert(!ChangeTracker.Entries().Any());
    }

    public override void Dispose()
    {
        base.Dispose();

        RoomChangeBuilder?.UnregisterFromGroups(Manager);
        RoomChangeBuilder = null;
        Manager = null!;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SyncGenerationStatsEntries();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SyncGenerationStatsEntries();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    ///     Reconciles every tracked <see cref="GenerationStats" />' owned <c>*Entries</c>
    ///     collections with their in-memory dictionaries so the change tracker observes
    ///     the INotifyCollectionChanged / INotifyPropertyChanged events for pending edits
    ///     before <see cref="DbContext.SaveChanges()" /> generates SQL.
    /// </summary>
    private void SyncGenerationStatsEntries()
    {
        foreach (var entry in ChangeTracker.Entries<GenerationStats>())
        {
            entry.Entity.SyncEntries();
        }
    }
    
    public static readonly ValueConverter<MapFeature[], byte[]> MapFeatureArrayConverter = CreateByteEnumArrayConverter<MapFeature>();

    public static readonly ValueComparer<MapFeature[]> MapFeatureArrayComparer = CreateByteEnumArrayComparer<MapFeature>();

    public static readonly ValueConverter<DirectionFlags[], byte[]> DirectionFlagsArrayConverter = CreateByteEnumArrayConverter<DirectionFlags>();

    public static readonly ValueComparer<DirectionFlags[]> DirectionFlagsArrayComparer = CreateByteEnumArrayComparer<DirectionFlags>();

    /// <summary>
    ///     Builds a <see cref="ValueConverter" /> between a <c>TEnum[]</c> and a <c>byte[]</c>
    ///     for any <see cref="byte" />-backed enum. Element-by-element copy with a per-element
    ///     cast — analyzer-friendly (CA2018 disallows <see cref="Buffer.BlockCopy" /> with
    ///     <c>array.Length</c>) and the JIT collapses it to a tight loop.
    /// </summary>
    private static ValueConverter<TEnum[], byte[]> CreateByteEnumArrayConverter<TEnum>()
        where TEnum : struct, Enum
        => new(
            v => EnumArrayToBytes(v),
            v => BytesToEnumArray<TEnum>(v));

    /// <summary>
    ///     Builds a <see cref="ValueComparer" /> for <c>TEnum[]</c>. The default reference
    ///     comparer would miss in-place mutations of the room's terrain/neighbor arrays;
    ///     element-wise equality + per-element hashing fixes that. Snapshot is intentionally
    ///     unsupported because we don't take property snapshots for these large arrays.
    /// </summary>
    private static ValueComparer<TEnum[]> CreateByteEnumArrayComparer<TEnum>()
        where TEnum : struct, Enum
        => new(
            (a, b) => ByteEnumArraysEqual(a, b),
            v => HashByteEnumArray(v),
            v => ThrowSnapshotNotSupported(v));

    // Full notification support is implemented, so snapshot methods shouldn't be used.
    private static T ThrowSnapshotNotSupported<T>(T value)
        => throw new NotSupportedException($"Cloning of {typeof(T).Name} is not supported.");

    private static byte[] EnumArrayToBytes<TEnum>(TEnum[] source) where TEnum : struct, Enum
    {
        var copy = new byte[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            copy[i] = Unsafe.As<TEnum, byte>(ref source[i]);
        }

        return copy;
    }

    private static TEnum[] BytesToEnumArray<TEnum>(byte[] source) where TEnum : struct, Enum
    {
        var copy = new TEnum[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            copy[i] = Unsafe.As<byte, TEnum>(ref source[i]);
        }

        return copy;
    }

    private static bool ByteEnumArraysEqual<T>(T[]? a, T[]? b) where T : struct, Enum
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a == null || b == null || a.Length != b.Length)
        {
            return false;
        }

        for (var i = 0; i < a.Length; i++)
        {
            if (!a[i].Equals(b[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static int HashByteEnumArray<T>(T[] array) where T : struct, Enum
    {
        var hash = new HashCode();
        for (var i = 0; i < array.Length; i++)
        {
            hash.Add(array[i]);
        }

        return hash.ToHashCode();
    }

    #region GenerationStats owned-collection mapping

    // Each Dictionary<,> on GenerationStats is persisted via a sibling
    // private ObservableCollection<...Entry> field exposed through a private
    // navigation property. EF accesses the navigation via PropertyAccessMode.Field
    // (so materialization doesn't invoke the reconciling getter on an empty
    // entries collection). The getter lazily reconciles the collection with the
    // dictionary, firing INotifyCollectionChanged events for adds/removes and
    // INotifyPropertyChanged events for Count updates. EnsureDictionariesInitialized()
    // in GenerationStats does the reverse on first dictionary access after EF
    // materializes the entity. SaveChanges overrides call GenerationStats.SyncEntries()
    // on every tracked instance so the change tracker observes the latest events
    // before SQL generation.

    private static void ConfigureGenerationStats<TOwner, TStats>(
        OwnedNavigationBuilder<TOwner, TStats> ob)
        where TOwner : class
        where TStats : GenerationStats
    {
        // Tell EF Core to ignore the public dictionary surface: that side
        // is reconstructed in memory from the private *Entries collections.
        ob.Ignore(s => s.FragmentInstancesByName);
        ob.Ignore(s => s.FragmentInstancesByTag);
        ob.Ignore(s => s.FragmentInstancesByFeature);
        ob.Ignore(s => s.CreatureInstancesByName);
        ob.Ignore(s => s.CreatureInstancesByFlag);
        ob.Ignore(s => s.ItemInstancesByName);
        ob.Ignore(s => s.ItemInstancesByType);
        ob.Ignore(s => s.RoomCountsByFeature);

        ConfigureCountBy<string, TOwner, TStats>(ob, "FragmentInstancesByNameEntries");
        ConfigureCountBy<string, TOwner, TStats>(ob, "FragmentInstancesByTagEntries");
        ConfigureCountBy<RoomFeatures, TOwner, TStats>(ob, "FragmentInstancesByFeatureEntries");
        ConfigureCountBy<string, TOwner, TStats>(ob, "CreatureInstancesByNameEntries");
        ConfigureCountBy<CreatureGenerationFlags, TOwner, TStats>(ob, "CreatureInstancesByFlagEntries");
        ConfigureCountBy<string, TOwner, TStats>(ob, "ItemInstancesByNameEntries");
        ConfigureCountBy<ItemType, TOwner, TStats>(ob, "ItemInstancesByTypeEntries");
        ConfigureCountBy<RoomFeatures, TOwner, TStats>(ob, "RoomCountsByFeatureEntries");
    }

    private static void ConfigureLevelGenerationStats<TOwner>(
        OwnedNavigationBuilder<TOwner, LevelGenerationStats> ob)
        where TOwner : class
    {
        ConfigureGenerationStats(ob);

        ob.Ignore(s => s.FragmentRoomFailures);
        ob.Ignore(s => s.FragmentInstancesByVariant);

        ConfigureCountBy<string, TOwner, LevelGenerationStats>(ob, "FragmentRoomFailuresEntries");
        ConfigureCountBy<string, TOwner, LevelGenerationStats>(ob, "FragmentInstancesByVariantEntries");
    }

    private static void ConfigureCountBy<TKey, TOwner, TStats>(
        OwnedNavigationBuilder<TOwner, TStats> ob,
        string navigation)
        where TKey : notnull
        where TOwner : class
        where TStats : GenerationStats
    {
        ob.OwnsMany<KeyCountPair<TKey>>(
            navigation,
            cb =>
            {
                cb.Property(e => e.Key);
                cb.Property(e => e.Count);
            });
        ob.Navigation(navigation).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    #endregion
}

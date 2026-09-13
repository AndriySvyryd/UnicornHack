using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Data;

public class GameDbContext : DbContext, IRepository
{
    private sealed class LoadedEntityId
    {
        public int Id { get; init; }
        public bool IsEffect { get; init; }
    }

    private static readonly Expression<Func<GameDbContext, int, IQueryable<int>>> RoomIdsQuery =
        (GameDbContext context, int id) =>
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
                        (_, connectionComponent) => connectionComponent.TargetRoomId));

    private static readonly Func<GameDbContext, int, IEnumerable<int>> GetRoomIds =
        EF.CompileQuery(RoomIdsQuery);

    private static readonly Func<GameDbContext, int, IAsyncEnumerable<int>> GetRoomIdsAsync =
        EF.CompileAsyncQuery(RoomIdsQuery);

    private static readonly Expression<Func<GameDbContext, int, int[], IQueryable<int>>> RoomResidentIdsQuery =
        (GameDbContext context, int gameId, int[] roomIds) =>
            context.Entities
                .Where(entity => entity.GameId == gameId && roomIds.Contains(entity.Id))
                .Select(entity => entity.Id)
                .Concat(context.Entities
                    .Where(entity => entity.GameId == gameId && roomIds.Contains(entity.Position!.RoomId))
                    .Select(entity => entity.Id))
                .Distinct();

    private static readonly Func<GameDbContext, int, int[], IEnumerable<int>> GetRoomResidentIds =
        EF.CompileQuery(RoomResidentIdsQuery);

    private static readonly Func<GameDbContext, int, int[], IAsyncEnumerable<int>> GetRoomResidentIdsAsync =
        EF.CompileAsyncQuery(RoomResidentIdsQuery);

    private static readonly Expression<Func<GameDbContext, int, int[], IQueryable<LoadedEntityId>>> TopEntityIdsQuery =
        (GameDbContext context, int gameId, int[] roomResidentIds) =>
            context.AbilityComponents
                .Where(ability => ability.GameId == gameId
                                  && ability.OwnerId.HasValue
                                  && roomResidentIds.Contains(ability.OwnerId.Value))
                .Select(ability => new LoadedEntityId { Id = ability.EntityId, IsEffect = false })
                .Concat(context.EffectComponents
                    .Where(effect => effect.GameId == gameId
                                     && effect.ContainingAbilityId.HasValue
                                     && context.AbilityComponents.Any(ability =>
                                         ability.GameId == gameId
                                         && ability.EntityId == effect.ContainingAbilityId.Value
                                         && ability.OwnerId.HasValue
                                         && roomResidentIds.Contains(ability.OwnerId.Value)))
                    .Select(effect => new LoadedEntityId { Id = effect.EntityId, IsEffect = true }));

    private static readonly Func<GameDbContext, int, int[], IEnumerable<LoadedEntityId>> GetTopEntityIds =
        EF.CompileQuery(TopEntityIdsQuery);

    private static readonly Func<GameDbContext, int, int[], IAsyncEnumerable<LoadedEntityId>> GetTopEntityIdsAsync =
        EF.CompileAsyncQuery(TopEntityIdsQuery);

    private static readonly Expression<Func<GameDbContext, int, int[], IQueryable<LoadedEntityId>>> NestedEntityIdsQuery =
        (GameDbContext context, int gameId, int[] topEffectIds) =>
            context.AbilityComponents
                .Where(ability => ability.GameId == gameId
                                  && ability.OwnerId.HasValue
                                  && topEffectIds.Contains(ability.OwnerId.Value))
                .Select(ability => new LoadedEntityId { Id = ability.EntityId, IsEffect = false })
                .Concat(context.EffectComponents
                    .Where(effect => effect.GameId == gameId
                                     && effect.ContainingAbilityId.HasValue
                                     && topEffectIds.Contains(effect.ContainingAbilityId.Value))
                    .Select(effect => new LoadedEntityId { Id = effect.EntityId, IsEffect = true }))
                .Concat(context.EffectComponents
                    .Where(effect => effect.GameId == gameId
                                     && effect.ContainingAbilityId.HasValue
                                     && context.AbilityComponents.Any(ability =>
                                         ability.GameId == gameId
                                         && ability.EntityId == effect.ContainingAbilityId.Value
                                         && ability.OwnerId.HasValue
                                         && topEffectIds.Contains(ability.OwnerId.Value)))
                    .Select(effect => new LoadedEntityId { Id = effect.EntityId, IsEffect = true }));

    private static readonly Func<GameDbContext, int, int[], IEnumerable<LoadedEntityId>> GetNestedEntityIds =
        EF.CompileQuery(NestedEntityIdsQuery);

    private static readonly Func<GameDbContext, int, int[], IAsyncEnumerable<LoadedEntityId>> GetNestedEntityIdsAsync =
        EF.CompileAsyncQuery(NestedEntityIdsQuery);

    private static readonly Expression<Func<GameDbContext, int, int[], IQueryable<GameEntity>>> EntitiesQuery =
        (GameDbContext context, int gameId, int[] entityIds) =>
            context.Entities.Where(entity => entity.GameId == gameId && entityIds.Contains(entity.Id))
                .Include(entity => entity.Room!).ThenInclude(room => room.Level).ThenInclude(level => level.Branch)
                .Include(entity => entity.Position)
                .Include(entity => entity.Connection)
                .Include(entity => entity.Being)
                .Include(entity => entity.Player)
                .Include(entity => entity.AI)
                .Include(entity => entity.Physical)
                .Include(entity => entity.Item)
                .Include(entity => entity.Knowledge)
                .Include(entity => entity.Ability)
                .Include(entity => entity.Effect)
                .Include(entity => entity.ActiveEffects);

    private static readonly Func<GameDbContext, int, int[], IEnumerable<GameEntity>> GetEntities =
        EF.CompileQuery(EntitiesQuery);

    private static readonly Func<GameDbContext, int, int[], IAsyncEnumerable<GameEntity>> GetEntitiesAsync =
        EF.CompileAsyncQuery(EntitiesQuery);

    private static readonly Expression<Func<GameDbContext, int, int[], int, IQueryable<LogEntry>>>
        RecentLogEntriesQuery =
            (GameDbContext context, int gameId, int[] playerIds, int entryCount) =>
            context.LogEntries
                .Where(entry => entry.GameId == gameId
                    && playerIds.Contains(entry.PlayerId)
                    && context.LogEntries
                        .Where(other => other.GameId == gameId && other.PlayerId == entry.PlayerId)
                        .OrderByDescending(other => other.Tick)
                        .ThenByDescending(other => other.Id)
                        .Take(entryCount)
                        .Contains(entry));

    private static readonly Func<GameDbContext, int, int[], int, IEnumerable<LogEntry>> GetRecentLogEntries =
        EF.CompileQuery(RecentLogEntriesQuery);

    private static readonly Func<GameDbContext, int, int[], int, IAsyncEnumerable<LogEntry>>
        GetRecentLogEntriesAsync =
        EF.CompileAsyncQuery(RecentLogEntriesQuery);

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

    public DbSet<ActiveEffectsComponent> ActiveEffectsComponents
    {
        get;
        set;
    } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: Use pre-convention model configuration to ignore all collection types

        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty);

        modelBuilder.Ignore<Utils.MessagingECS.Entity>();
        modelBuilder.Ignore<Utils.MessagingECS.Component>();
        modelBuilder.Ignore<GameComponent>();

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
            eb.HasMany<ActiveEffectsComponent>()
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
                .OnDelete(DeleteBehavior.Cascade);
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
            eb.HasOne(e => e.ActiveEffects).WithOne(c => c.Entity)
                .HasForeignKey<ActiveEffectsComponent>(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<AbilityComponent>(eb =>
        {
            eb.Ignore(c => c.OwnerEntity);
            eb.Ignore(c => c.Template);
            eb.Ignore(c => c.AccuracyFunction);
            eb.Ignore(c => c.DelayFunction);
            eb.Ignore(c => c.Effects);
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<AIComponent>(eb =>
        {
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<PlayerComponent>(eb =>
        {
            eb.Property(p => p.ProperName).IsRequired();
            eb.HasIndex(p => p.ProperName).IsUnique();
            eb.HasKey(p => new { p.GameId, p.EntityId });
            eb.OwnsOne(p => p.PendingChoice, choice =>
            {
                choice.Property(value => value.ChoiceType).IsRequired();
                choice.Property(value => value.PendingChoiceOptions).IsRequired();
            });
            eb.Navigation(p => p.PendingChoice).IsRequired(false);
        });

        modelBuilder.Entity<BeingComponent>(eb =>
        {
            eb.Ignore(c => c.IsAlive);
            eb.Ignore(c => c.SlottedAbilities);
            eb.Ignore(c => c.Abilities);
            eb.Ignore(c => c.Races);
            eb.Ignore(c => c.Items);
            eb.Ignore(c => c.HitPointMaximum);
            eb.Ignore(c => c.EnergyPointMaximum);
            eb.Ignore(c => c.Perception);
            eb.Ignore(c => c.Might);
            eb.Ignore(c => c.Speed);
            eb.Ignore(c => c.Focus);
            eb.Ignore(c => c.EnergyRegeneration);
            eb.Ignore(c => c.Regeneration);
            eb.Ignore(c => c.HealingResistance);
            eb.Ignore(c => c.EnergyRestorationResistance);
            eb.Ignore(c => c.Hindrance);
            eb.Ignore(c => c.Accuracy);
            eb.Ignore(c => c.Evasion);
            eb.Ignore(c => c.Deflection);
            eb.Ignore(c => c.Armor);
            eb.Ignore(c => c.PhysicalResistance);
            eb.Ignore(c => c.MagicResistance);
            eb.Ignore(c => c.AcidResistance);
            eb.Ignore(c => c.BleedingResistance);
            eb.Ignore(c => c.ColdResistance);
            eb.Ignore(c => c.ElectricityResistance);
            eb.Ignore(c => c.FireResistance);
            eb.Ignore(c => c.LightResistance);
            eb.Ignore(c => c.PsychicResistance);
            eb.Ignore(c => c.SonicResistance);
            eb.Ignore(c => c.StunResistance);
            eb.Ignore(c => c.DoomResistance);
            eb.Ignore(c => c.BindResistance);
            eb.Ignore(c => c.BlindResistance);
            eb.Ignore(c => c.CorrodeResistance);
            eb.Ignore(c => c.CrippleResistance);
            eb.Ignore(c => c.DazeResistance);
            eb.Ignore(c => c.DeafenResistance);
            eb.Ignore(c => c.DecayResistance);
            eb.Ignore(c => c.DisorientResistance);
            eb.Ignore(c => c.EnervateResistance);
            eb.Ignore(c => c.EngulfResistance);
            eb.Ignore(c => c.ExposeResistance);
            eb.Ignore(c => c.FascinateResistance);
            eb.Ignore(c => c.FrightenResistance);
            eb.Ignore(c => c.MuteResistance);
            eb.Ignore(c => c.ParalyzeResistance);
            eb.Ignore(c => c.SedateResistance);
            eb.Ignore(c => c.SlowResistance);
            eb.Ignore(c => c.StaggerResistance);
            eb.Ignore(c => c.StickResistance);
            eb.Ignore(c => c.SuffocateResistance);
            eb.Ignore(c => c.TauntResistance);
            eb.Ignore(c => c.ToppleResistance);
            eb.Ignore(c => c.WetResistance);
            eb.Ignore(c => c.ToxinResistance);
            eb.Ignore(c => c.VoidResistance);
            eb.Ignore(c => c.WaterResistance);
            eb.Ignore(c => c.HeadType);
            eb.Ignore(c => c.TorsoType);
            eb.Ignore(c => c.UpperExtremities);
            eb.Ignore(c => c.LowerExtremities);
            eb.Ignore(c => c.BackExtremities);
            eb.Ignore(c => c.Visibility);
            eb.Ignore(c => c.PrimaryFOVQuadrants);
            eb.Ignore(c => c.TotalFOVQuadrants);
            eb.Ignore(c => c.PrimaryVisionRange);
            eb.Ignore(c => c.SecondaryVisionRange);
            eb.Ignore(c => c.Infravision);
            eb.Ignore(c => c.InvisibilityDetection);
            eb.Ignore(c => c.VisibleTerrain);
            eb.Ignore(c => c.VisibleTerrainIsCurrent);
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<PhysicalComponent>(eb =>
        {
            eb.Ignore(c => c.Abilities);
            eb.Ignore(c => c.Items);
            eb.Ignore(c => c.Material);
            eb.Ignore(c => c.Weight);
            eb.Ignore(c => c.Size);
            eb.Ignore(c => c.Capacity);
            eb.Ignore(c => c.Infravisible);
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<EffectComponent>(eb =>
        {
            eb.Ignore(c => c.SourceEffect);
            eb.Ignore(c => c.SourceAbility);
            eb.Ignore(c => c.ContainingAbility);
            eb.Ignore(c => c.AmountFunction);
            eb.Ignore(c => c.SecondaryAmountFunction);
            eb.Ignore(c => c.DurationAmountFunction);
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<ItemComponent>(eb =>
        {
            eb.Ignore(c => c.ContainerEntity);
            eb.Ignore(c => c.Abilities);
            eb.Ignore(c => c.Items);
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<KnowledgeComponent>(eb =>
        {
            eb.Ignore(c => c.KnownEntity);
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });

        modelBuilder.Entity<ActiveEffectsComponent>(eb =>
        {
            eb.HasKey(c => new { c.GameId, c.EntityId });
            eb.Ignore(c => c.Abilities);
            eb.OwnsMany(c => c.PropertyEffects, ib =>
            {
                ib.WithOwner(e => (ActiveEffectsComponent)e.ContainingComponent!)
                    .HasForeignKey(c => new { c.GameId, c.EntityId });
                ib.Ignore(e => e.ContainingPropertyName);
                ib.Ignore(e => e.Metadata);
                ib.HasKey(c => new { c.GameId, c.EntityId, c.InstanceId });
                ib.Property(effect => effect.EffectType);
                ib.Property(effect => effect.TargetName);
                ib.Property(effect => effect.InstanceId).ValueGeneratedNever();
            });
            eb.OwnsMany(c => c.ComponentEffects, ib =>
            {
                ib.WithOwner(e => (ActiveEffectsComponent)e.ContainingComponent!)
                    .HasForeignKey(c => new { c.GameId, c.EntityId });
                ib.Ignore(e => e.ContainingPropertyName);
                ib.HasKey(c => new { c.GameId, c.EntityId, c.InstanceId });
                ib.Property(effect => effect.EffectType);
                ib.Property(effect => effect.TargetName);
                ib.Property(effect => effect.InstanceId).ValueGeneratedNever();
            });
            eb.OwnsMany(c => c.Accumulators, ib =>
            {
                ib.WithOwner(e => (ActiveEffectsComponent)e.ContainingComponent!)
                    .HasForeignKey(c => new { c.GameId, c.EntityId });
                ib.Ignore(e => e.ContainingPropertyName);
                ib.HasKey(c => new { c.GameId, c.EntityId, c.EffectType });
                ib.Property(accumulator => accumulator.EffectType).ValueGeneratedNever();
            });
        });

        modelBuilder.Entity<ConnectionComponent>(eb =>
        {
            eb.Ignore(c => c.TargetCell);
            eb.Ignore(c => c.TargetRoomEntity);
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
            eb.Ignore(c => c.MovementDelay);
            eb.Ignore(c => c.TurningDelay);
            eb.HasKey(c => new { c.GameId, c.EntityId });
        });
    }

    public Game LoadGame(int gameId)
        => LoadGame(gameId, useAsync: false).GetAwaiter().GetResult();

    public ValueTask<Game> LoadGameAsync(
        int gameId, CancellationToken cancellationToken = default)
        => LoadGame(gameId, useAsync: true, cancellationToken);

    private async ValueTask<Game> LoadGame(
        int gameId, bool useAsync, CancellationToken cancellationToken = default)
    {
        // For each player: emit the player's current room id, plus - by walking
        // player -> position (player's room id) -> player's room (BranchName, Depth)
        // -> every room on that level -> that room's connections - each connection's
        // target room id. Union deduplicates.
        var roomIds = useAsync
            ? await ToListAsync(GetRoomIdsAsync(this, gameId), cancellationToken)
                .ConfigureAwait(false)
            : await ToList(GetRoomIds(this, gameId));

        await LoadRooms(roomIds, useAsync, cancellationToken).ConfigureAwait(false);

        // Load the 10 most recent log entries per player.
        const int logEntryCount = 10;
        var playerIds = PlayerComponents.Local.Select(player => player.EntityId).ToArray();
        if (useAsync)
        {
            await LoadAsync(GetRecentLogEntriesAsync(this, gameId, playerIds, logEntryCount), cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await Load(GetRecentLogEntries(this, gameId, playerIds, logEntryCount));
        }

        return useAsync
            ? (await FindAsync<Game>([gameId], cancellationToken).ConfigureAwait(false))!
            : Find<Game>(gameId)!;
    }

    public void LoadRooms(IReadOnlyList<int> roomIds)
        => LoadRooms(roomIds, useAsync: false).GetAwaiter().GetResult();

    public ValueTask LoadRoomsAsync(
        IReadOnlyList<int> roomIds, CancellationToken cancellationToken = default)
        => LoadRooms(roomIds, useAsync: true, cancellationToken);

    private async ValueTask LoadRooms(
        IReadOnlyList<int> roomIds, bool useAsync, CancellationToken cancellationToken = default)
    {
        Manager.IsLoading = true;
        try
        {
            if (roomIds.Count == 0)
            {
                return;
            }

            var roomIdArray = roomIds as int[] ?? roomIds.ToArray();
            var gameId = Manager.Game.Id;

            // 1. Entities that ARE one of the requested rooms or are positioned in one.
            //    Split into two Concat branches so each becomes a single-predicate query
            //    against the roomIds parameter - see the nestedMix comment below for why.
            //    Distinct is required because a room entity also has itself in Position.
            var roomResidentIds = useAsync
                ? await ToListAsync(GetRoomResidentIdsAsync(this, gameId, roomIdArray), cancellationToken)
                    .ConfigureAwait(false)
                : await ToList(GetRoomResidentIds(this, gameId, roomIdArray));

            // 2. Top-level abilities AND top-level effects in one round-trip.
            //    - Abilities owned by an affectable (Kind = 0)
            //    - Effects either directly affecting an affectable, OR contained
            //      in an ability that itself is owned by an affectable (Kind = 1).
            //      The second branch is expressed as EXISTS against AbilityComponents
            //      so we don't need a materialized topAbilityIds list yet.
            var roomResidentIdArray = roomResidentIds.ToArray();
            var topMix = roomResidentIds.Count == 0
                ? []
                : useAsync
                    ? await ToListAsync(GetTopEntityIdsAsync(this, gameId, roomResidentIdArray), cancellationToken)
                        .ConfigureAwait(false)
                    : await ToList(GetTopEntityIds(this, gameId, roomResidentIdArray));

            var topAbilityIds = topMix.Where(entry => !entry.IsEffect).Select(entry => entry.Id).ToList();
            var topEffectIds = topMix.Where(entry => entry.IsEffect).Select(entry => entry.Id).ToList();

            // 3. Nested abilities and effects (one more graph level) in one round-trip.
            //    Each branch of the Concat is a single-predicate query against the
            //    topEffectIds list - this is deliberate. A single Where(ec => A || B || C)
            //    where each clause references the same OPENJSON parameter forces SQL
            //    Server into nested loops that rebuild OPENJSON per outer row. Splitting
            //    into UNION ALL lets each branch use a hash semi join that builds
            //    OPENJSON once and hash-probes the source table. Duplicate effect ids
            //    across branches are removed by the client-side Distinct() below.
            //    - Abilities owned by a top-level effect (Kind = 0)
            //    - Effects directly affecting a top-level effect (Kind = 1)
            //    - Effects directly affecting a top-level ability (Kind = 1)
            //    - Effects contained in a top-level effect (Kind = 1)
            //    - Effects contained in a nested ability - expressed as EXISTS so we
            //      don't need a materialized nestedAbilityIds list yet (Kind = 1).
            var topEffectIdArray = topEffectIds.ToArray();
            var nestedMix = topEffectIds.Count == 0
                ? []
                : useAsync
                    ? await ToListAsync(GetNestedEntityIdsAsync(this, gameId, topEffectIdArray), cancellationToken)
                        .ConfigureAwait(false)
                    : await ToList(GetNestedEntityIds(this, gameId, topEffectIdArray));

            var nestedAbilityIds = nestedMix.Where(entry => !entry.IsEffect).Select(entry => entry.Id).ToList();
            var nestedEffectIds = nestedMix.Where(entry => entry.IsEffect)
                .Select(entry => entry.Id).Distinct().ToList();

            // 4. Load every reachable entity in a single query together with all of
            //    its components. Including every component navigation is safe even
            //    for entities that wouldn't normally carry them - the LEFT JOINs
            //    simply return nulls - and it guards against unexpected component
            //    combinations.
            var allEntityIds = roomResidentIds
                .Concat(topAbilityIds)
                .Concat(topEffectIds)
                .Concat(nestedAbilityIds)
                .Concat(nestedEffectIds)
                .Distinct()
                .ToArray();

            if (allEntityIds.Length > 0)
            {
                if (useAsync)
                {
                    await LoadAsync(GetEntitiesAsync(this, gameId, allEntityIds), cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await Load(GetEntities(this, gameId, allEntityIds));
                }
            }
        }
        finally
        {
            Manager.IsLoading = false;
        }

        foreach (var room in RoomComponents.Local)
        {
            room.EnsureInitialized();
        }

        foreach (var activeEffects in ActiveEffectsComponents.Local)
        {
            Manager.EffectApplicationSystem.RebuildDynamicMagnitudes(activeEffects.Entity);
            Manager.EngulferToVictimRelationship.Rebuild(activeEffects.Entity);
        }
    }

    private static ValueTask<List<T>> ToList<T>(IEnumerable<T> query)
        => ValueTask.FromResult(query.ToList());

    private static async ValueTask<List<T>> ToListAsync<T>(
        IAsyncEnumerable<T> query, CancellationToken cancellationToken)
    {
        var result = new List<T>();
        await foreach (var item in query.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            result.Add(item);
        }

        return result;
    }

    private static ValueTask Load<T>(IEnumerable<T> query)
    {
        foreach (var _ in query)
        {
        }

        return ValueTask.CompletedTask;
    }

    private static async ValueTask LoadAsync<T>(
        IAsyncEnumerable<T> query, CancellationToken cancellationToken)
    {
        await foreach (var _ in query.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
        }
    }

    public void DeleteLevel(GameLevel level)
        => Entry(level).State = EntityState.Deleted;

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
        => Clean(game, useAsync: false).GetAwaiter().GetResult();

    public ValueTask CleanAsync(Game game, CancellationToken cancellationToken = default)
        => Clean(game, useAsync: true, cancellationToken);

    private async ValueTask Clean(
        Game game, bool useAsync, CancellationToken cancellationToken = default)
    {
        var gameId = game.Id;
        game.ActingPlayerId = null;

        if (useAsync)
        {
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            SaveChanges();
        }

        ChangeTracker.Clear();

        await ExecuteDelete(RoomComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(Levels.Where(level => level.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(Branches.Where(branch => branch.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(LogEntries.Where(entry => entry.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(PlayerCommands.Where(command => command.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(AbilityComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(AIComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(PlayerComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(BeingComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(PhysicalComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(EffectComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(ItemComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(KnowledgeComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(ConnectionComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(PositionComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(ActiveEffectsComponents.Where(component => component.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(Entities.Where(entity => entity.GameId == gameId), useAsync, cancellationToken);
        await ExecuteDelete(Games.Where(candidate => candidate.Id == gameId), useAsync, cancellationToken);
    }

    private static ValueTask<int> ExecuteDelete<T>(
        IQueryable<T> query, bool useAsync, CancellationToken cancellationToken)
        where T : class
        => useAsync
            ? new ValueTask<int>(query.ExecuteDeleteAsync(cancellationToken))
            : ValueTask.FromResult(query.ExecuteDelete());

    public override void Dispose()
    {
        Manager = null!;
        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        Manager = null!;
        return base.DisposeAsync();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SyncGenerationStatsEntries();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SyncGenerationStatsEntries();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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

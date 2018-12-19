using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Hubs;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;

namespace UnicornHack.Data
{
    public class GameDbContext : DbContext, IRepository
    {
        private Func<GameDbContext, int, IEnumerable<int>> _getLevels;

        // ReSharper disable once SuggestBaseTypeForParameter
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public GameManager Manager { get; set; }
        public GameSnapshot Snapshot { get; set; }

        public DbSet<Game> Games { get; set; }
        public DbSet<GameBranch> Branches { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<PlayerCommand> PlayerCommands { get; set; }
        public DbSet<GameEntity> Entities { get; set; }
        public DbSet<AbilityComponent> AbilityComponents { get; set; }
        public DbSet<AIComponent> AIComponents { get; set; }
        public DbSet<PlayerComponent> PlayerComponents { get; set; }
        public DbSet<BeingComponent> BeingComponents { get; set; }
        public DbSet<PhysicalComponent> PhysicalComponents { get; set; }
        public DbSet<RaceComponent> RaceComponents { get; set; }
        public DbSet<EffectComponent> EffectComponents { get; set; }
        public DbSet<ItemComponent> ItemComponents { get; set; }
        public DbSet<KnowledgeComponent> KnowledgeComponents { get; set; }
        public DbSet<ConnectionComponent> ConnectionComponents { get; set; }
        public DbSet<LevelComponent> LevelComponents { get; set; }
        public DbSet<PositionComponent> PositionComponents { get; set; }
        public DbSet<SensorComponent> SensorComponents { get; set; }

        private IQueryable<GameEntity> AggregateEntities
            => Entities
                    .Include(e => e.Level).ThenInclude(l => l.Branch)
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
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy
                .ChangingAndChangedNotificationsWithOriginalValues);

            modelBuilder.Entity<Game>(eb =>
            {
                eb.Ignore(g => g.Services);
                eb.Ignore(g => g.Manager);
                eb.Ignore(g => g.Repository);

                eb.Property(g => g.Id)
                    .ValueGeneratedOnAdd();
                eb.OwnsOne(g => g.Random);
                eb.HasOne(g => g.ActingPlayer)
                    .WithOne()
                    .HasForeignKey<Game>(l => new
                    {
                        l.Id,
                        l.ActingPlayerId
                    })
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(g => g.Branches)
                    .WithOne(l => l.Game)
                    .HasForeignKey(l => l.GameId);
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
                eb.HasMany<LevelComponent>()
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
                eb.HasKey(b => new
                {
                    b.GameId,
                    b.Name
                });
                eb.HasMany(b => b.Levels)
                    .WithOne(l => l.Branch)
                    .HasForeignKey(l => new
                    {
                        l.GameId,
                        l.BranchName
                    });
            });

            modelBuilder.Entity<LogEntry>(eb =>
            {
                eb.HasKey(e => new
                {
                    e.GameId,
                    e.Id
                });
                eb.Property(l => l.Message).IsRequired();
                eb.HasOne<PlayerComponent>()
                    .WithMany(pc => pc.LogEntries)
                    .HasForeignKey(l => new
                    {
                        l.GameId,
                        l.PlayerId
                    });
            });

            modelBuilder.Entity<PlayerCommand>(eb =>
            {
                eb.HasKey(e => new
                {
                    e.GameId,
                    e.Id
                });
                eb.HasOne<PlayerComponent>()
                    .WithMany(pc => pc.CommandHistory)
                    .HasForeignKey(l => new
                    {
                        l.GameId,
                        l.PlayerId
                    });
            });

            modelBuilder.Entity<GameEntity>(eb =>
            {
                eb.Ignore(e => e.Manager);
                eb.Property("_referenceCount");
                eb.HasKey(e => new
                {
                    e.GameId,
                    e.Id
                });
                eb.HasOne(e => e.Ability).WithOne(c => c.Entity).HasForeignKey<AbilityComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.AI).WithOne(c => c.Entity).HasForeignKey<AIComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Being).WithOne(c => c.Entity).HasForeignKey<BeingComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Connection).WithOne(c => c.Entity).HasForeignKey<ConnectionComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Effect).WithOne(c => c.Entity).HasForeignKey<EffectComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Item).WithOne(c => c.Entity).HasForeignKey<ItemComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Knowledge).WithOne(c => c.Entity).HasForeignKey<KnowledgeComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Level).WithOne(c => c.Entity).HasForeignKey<LevelComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Physical).WithOne(c => c.Entity).HasForeignKey<PhysicalComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Position).WithOne(c => c.Entity).HasForeignKey<PositionComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Player).WithOne(c => c.Entity).HasForeignKey<PlayerComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Race).WithOne(c => c.Entity).HasForeignKey<RaceComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
                eb.HasOne(e => e.Sensor).WithOne(c => c.Entity).HasForeignKey<SensorComponent>(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<AbilityComponent>(eb =>
            {
                eb.Ignore(c => c.OwnerEntity);
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<AIComponent>(eb =>
            {
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<PlayerComponent>(pb =>
            {
                pb.HasIndex(p => p.ProperName).IsUnique();
                pb.HasKey(p => new
                {
                    p.GameId,
                    p.EntityId
                });
            });

            modelBuilder.Entity<BeingComponent>(eb =>
            {

                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<PhysicalComponent>(eb =>
            {
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<RaceComponent>(eb => {
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<EffectComponent>(eb =>
            {
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<ItemComponent>(eb => {
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<KnowledgeComponent>(eb =>
            {
                eb.Ignore(c => c.KnownEntity);
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<ConnectionComponent>(eb =>
            {
                eb.Ignore(c => c.TargetLevelCell);
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<LevelComponent>(eb =>
            {
                eb.Ignore(l => l.IndexToPoint);
                eb.Ignore(l => l.PointToIndex);
                eb.Ignore(l => l.TerrainChanges);
                eb.Ignore(l => l.KnownTerrainChanges);
                eb.Ignore(l => l.WallNeighboursChanges);
                eb.Ignore(l => l.VisibleTerrainSnapshot);
                eb.Ignore(l => l.VisibleTerrainChanges);
                eb.Ignore(l => l.VisibleNeighboursChanged);
                eb.Ignore(l => l.PathFinder);
                eb.Ignore(l => l.VisibilityCalculator);
                eb.Property("_tracked");

                eb.HasKey(l => new
                {
                    l.GameId,
                    l.EntityId
                });
                eb.OwnsOne(l => l.GenerationRandom);
            });

            modelBuilder.Entity<PositionComponent>(eb =>
            {
                eb.Ignore(c => c.LevelCell);
                eb.Ignore(c => c.LevelEntity);
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<SensorComponent>(eb =>
            {
                eb.Ignore(c => c.VisibleTerrain);
                eb.Ignore(c => c.VisibleTerrainIsCurrent);
                eb.Property("_tracked");
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });
        }

        public Game LoadGame(int gameId)
        {
            if (_getLevels == null)
            {
                _getLevels = EF.CompileQuery((GameDbContext context, int id) =>
                    context.PlayerComponents
                        .Where(playerComponent => playerComponent.GameId == id)
                        .Join(context.PositionComponents,
                            playerComponent => new
                            {
                                playerComponent.GameId,
                                playerComponent.EntityId
                            },
                            positionComponent => new
                            {
                                positionComponent.GameId,
                                positionComponent.EntityId
                            },
                            (_, positionComponent) => positionComponent.LevelId)
                        .Join(context.PositionComponents,
                            levelId => new
                            {
                                GameId = id,
                                LevelId = levelId
                            },
                            positionComponent => new
                            {
                                positionComponent.GameId,
                                positionComponent.LevelId
                            },
                            (_, positionComponent) => positionComponent)
                        .Join(context.ConnectionComponents,
                            positionComponent
                                => new
                                {
                                    positionComponent.GameId,
                                    positionComponent.EntityId
                                },
                            connectionComponent => new
                            {
                                connectionComponent.GameId,
                                connectionComponent.EntityId
                            },
                            (positionComponent, connectionComponent)
                                => new[] { positionComponent.LevelId, connectionComponent.TargetLevelId })
                        .SelectMany(l => l).Distinct());
            }

            LoadLevels(_getLevels(this, gameId).ToList());

            foreach (var player in PlayerComponents.Local.ToList())
            {
                LogEntries
                    .Where(e => e.GameId == player.GameId && e.PlayerId == player.EntityId)
                    .OrderByDescending(e => e.Tick)
                    .ThenByDescending(e => e.Id)
                    .Take(10)
                    .Load();
            }

            return Find<Game>(gameId);
        }

        public void LoadLevels(IReadOnlyList<int> levelIds)
        {
            Manager.IsLoading = true;
            // TODO: Perf: Compile queries
            var levelEntities = Entities.Where(levelEntity
                => levelIds.Contains(levelEntity.Id) || levelIds.Contains(levelEntity.Position.LevelId));

            var containedItems = levelEntities
                .Join(ItemComponents,
                    levelEntity => levelEntity.Id, itemComponent => itemComponent.ContainerId,
                    (_, itemComponent) => itemComponent)
                .Join(Entities,
                    itemComponent => itemComponent.EntityId, containedEntity => containedEntity.Id,
                    (_, containedEntity) => containedEntity);

            var nestedContainedItems = containedItems
                .Join(ItemComponents,
                    levelEntity => levelEntity.Id, itemComponent => itemComponent.ContainerId,
                    (_, itemComponent) => itemComponent)
                .Join(Entities,
                    itemComponent => itemComponent.EntityId, containedEntity => containedEntity.Id,
                    (_, containedEntity) => containedEntity);

            // TODO: Query additional nested items

            var primaryNaturalWeapons = levelEntities
                .Join(BeingComponents,
                    levelEntity => levelEntity.Id, beingComponent => beingComponent.EntityId,
                    (_, beingComponent) => beingComponent)
                .Join(Entities,
                    beingComponent => beingComponent.PrimaryNaturalWeaponId, weaponEntity => weaponEntity.Id,
                    (_, weaponEntity) => weaponEntity);

            var secondaryNaturalWeapons = levelEntities
                .Join(BeingComponents,
                    levelEntity => levelEntity.Id, beingComponent => beingComponent.EntityId,
                    (_, beingComponent) => beingComponent)
                .Join(Entities,
                    beingComponent => beingComponent.SecondaryNaturalWeaponId, weaponEntity => weaponEntity.Id,
                    (_, weaponEntity) => weaponEntity);

            var affectableEntities = levelEntities
                .Concat(containedItems)
                .Concat(nestedContainedItems)
                .Concat(primaryNaturalWeapons)
                .Concat(secondaryNaturalWeapons);

            var appliedEffects = affectableEntities
                .Join(EffectComponents,
                    affectableEntity => affectableEntity.Id, effectComponent => effectComponent.AffectedEntityId,
                    (_, effectComponent) => effectComponent);

            var abilities = affectableEntities
                .Join(AbilityComponents,
                    affectableEntity => affectableEntity.Id, abilityComponent => abilityComponent.OwnerId,
                    (_, abilityComponent) => abilityComponent);

            var effects = abilities
                .Join(EffectComponents,
                    abilityComponent => abilityComponent.EntityId, effectComponent => effectComponent.ContainingAbilityId,
                    (_, effectComponent) => effectComponent);

            var nestedAbilityEffects = effects
                .Join(EffectComponents,
                    effectComponent => effectComponent.EntityId, nestedEffectComponent => nestedEffectComponent.ContainingAbilityId,
                    (_, nestedEffectComponent) => nestedEffectComponent);

            var effectItemAppliedEffects = effects
                .Join(EffectComponents,
                    effectComponent => effectComponent.EntityId, appliedEffectComponent => appliedEffectComponent.AffectedEntityId,
                    (_, effectComponent) => effectComponent);

            var effectItemAbilities = effects
                .Join(AbilityComponents,
                    effectComponent => effectComponent.EntityId, abilityComponent => abilityComponent.OwnerId,
                    (_, abilityComponent) => abilityComponent);

            var effectItemAbilityEffects = effectItemAbilities
                .Join(EffectComponents,
                    abilityComponent => abilityComponent.EntityId, effectComponent => effectComponent.ContainingAbilityId,
                    (_, effectComponent) => effectComponent);

            levelEntities
                .Include(e => e.Level).ThenInclude(l => l.Branch)
                .Include(e => e.Position)
                .Include(e => e.Connection)
                .Include(e => e.Being)
                .Include(e => e.Player)
                .Include(e => e.AI)
                .Include(e => e.Sensor)
                .Include(e => e.Physical)
                .Include(e => e.Item)
                .Include(e => e.Knowledge)
                .Load();

            containedItems
                .Include(e => e.Physical)
                .Include(e => e.Item)
                .Load();

            nestedContainedItems
                .Include(e => e.Physical)
                .Include(e => e.Item)
                .Load();

            primaryNaturalWeapons
                .Include(e => e.Physical)
                .Include(e => e.Item)
                .Load();

            secondaryNaturalWeapons
                .Include(e => e.Physical)
                .Include(e => e.Item)
                .Load();

            abilities
                .Join(Entities, abilityComponent => abilityComponent.EntityId,
                    abilityEntity => abilityEntity.Id,
                    (_, abilityEntity) => abilityEntity)
                .Include(e => e.Ability)
                .Load();

            appliedEffects
                .Join(Entities,
                    effectComponent => effectComponent.EntityId, effectEntity => effectEntity.Id,
                    (_, effectEntity) => effectEntity)
                .Include(e => e.Effect)
                .Include(e => e.Race)
                .Include(e => e.Ability)
                .Load();

            effects
                .Join(Entities,
                    effectComponent => effectComponent.EntityId, effectEntity => effectEntity.Id,
                    (_, effectEntity) => effectEntity)
                .Include(e => e.Effect)
                .Include(e => e.Physical)
                .Include(e => e.Item)
                .Include(e => e.Ability)
                .Load();

            nestedAbilityEffects
                .Join(Entities,
                    effectComponent => effectComponent.EntityId, effectEntity => effectEntity.Id,
                    (_, effectEntity) => effectEntity)
                .Include(e => e.Effect)
                .Include(e => e.Ability)
                .Load();

            effectItemAppliedEffects
                .Join(Entities,
                    effectComponent => effectComponent.EntityId, effectEntity => effectEntity.Id,
                    (_, effectEntity) => effectEntity)
                .Include(e => e.Effect)
                .Include(e => e.Ability)
                .Load();

            effectItemAbilities
                .Join(Entities,
                    abilityComponent => abilityComponent.EntityId, abilityEntity => abilityEntity.Id,
                    (_, effectEntity) => effectEntity)
                .Include(e => e.Ability)
                .Load();

            effectItemAbilityEffects
                .Join(Entities,
                    effectComponent => effectComponent.EntityId, effectEntity => effectEntity.Id,
                    (_, effectEntity) => effectEntity)
                .Include(e => e.Effect)
                .Include(e => e.Ability)
                .Load();

            Manager.IsLoading = false;

            foreach (var level in LevelComponents.Local)
            {
                level.EnsureInitialized();
            }
        }

        void IRepository.Add<T>(T entity)
            => Add(entity);

        void IRepository.Remove(object entity)
            => Remove(entity);

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
            foreach (var component in LevelComponents.Where(b => b.GameId == game.Id))
            {
                LevelComponents.Remove(component);
            }

            foreach (var branch in Branches.Where(b => b.GameId == game.Id))
            {
                Branches.Remove(branch);
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
        }
    }
}

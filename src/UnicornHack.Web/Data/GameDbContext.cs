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

            modelBuilder.Entity<AbilityComponent>(eb => eb.HasKey(c => new
            {
                c.GameId,
                c.EntityId
            }));

            modelBuilder.Entity<AIComponent>(eb => eb.HasKey(c => new
            {
                c.GameId,
                c.EntityId
            }));

            modelBuilder.Entity<PlayerComponent>(pb =>
            {
                pb.HasIndex(p => p.ProperName).IsUnique();
                pb.HasKey(p => new
                {
                    p.GameId,
                    p.EntityId
                });
            });

            modelBuilder.Entity<BeingComponent>(eb => eb.HasKey(c => new
            {
                c.GameId,
                c.EntityId
            }));

            modelBuilder.Entity<PhysicalComponent>(eb => eb.HasKey(c => new
            {
                c.GameId,
                c.EntityId
            }));

            modelBuilder.Entity<RaceComponent>(eb => eb.HasKey(c => new
            {
                c.GameId,
                c.EntityId
            }));

            modelBuilder.Entity<EffectComponent>(eb => eb.HasKey(c => new
            {
                c.GameId,
                c.EntityId
            }));

            modelBuilder.Entity<ItemComponent>(eb => eb.HasKey(c => new
            {
                c.GameId,
                c.EntityId
            }));

            modelBuilder.Entity<KnowledgeComponent>(eb =>
            {
                eb.Ignore(c => c.KnownEntity);
                eb.HasKey(c => new
                {
                    c.GameId,
                    c.EntityId
                });
            });

            modelBuilder.Entity<ConnectionComponent>(eb =>
            {
                eb.Ignore(c => c.TargetLevelCell);
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
                            (_, positionComponent) => positionComponent)
                        .Join(context.ConnectionComponents,
                            positionComponent
                                => new
                                {
                                    positionComponent.GameId,
                                    EntityId = positionComponent.LevelId
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

            foreach (var player in PlayerComponents.Local)
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
            // TODO: Perf: Compile queries
            var levelEntities = AggregateEntities.Where(levelEntity
                => levelIds.Contains(levelEntity.Id) || levelIds.Contains(levelEntity.Position.LevelId));

            var containedItems = levelEntities
                .Join(ItemComponents,
                    levelEntity => levelEntity.Id, itemComponent => itemComponent.ContainerId,
                    (_, itemComponent) => itemComponent)
                .Join(AggregateEntities,
                    itemComponent => itemComponent.EntityId, containedEntity => containedEntity.Id,
                    (_, containedEntity) => containedEntity);

            var nestedContainedItems = containedItems
                .Join(ItemComponents,
                    levelEntity => levelEntity.Id, itemComponent => itemComponent.ContainerId,
                    (_, itemComponent) => itemComponent)
                .Join(AggregateEntities,
                    itemComponent => itemComponent.EntityId, containedEntity => containedEntity.Id,
                    (_, containedEntity) => containedEntity);

            // TODO: Query additional nested items

            var affectableEntities = levelEntities.Concat(containedItems).Concat(nestedContainedItems);

            var appliedEffects = affectableEntities
                .Join(EffectComponents, affectableEntity => affectableEntity.Id,
                    effectComponent => effectComponent.AffectedEntityId,
                    (_, effectComponent) => effectComponent);

            var abilityEntities = affectableEntities
                .Join(AbilityComponents, affectableEntity => affectableEntity.Id,
                    abilityComponent => abilityComponent.OwnerId,
                    (_, abilityComponent) => abilityComponent)
                .Join(AggregateEntities, abilityComponent => abilityComponent.EntityId,
                    abilityEntity => abilityEntity.Id,
                    (_, abilityEntity) => abilityEntity);

            var abilityDefinitionEntities = appliedEffects
                .Join(AbilityComponents, abilityEffect => abilityEffect.SourceAbilityId,
                    abilityComponent => abilityComponent.EntityId,
                    (_, abilityComponent) => abilityComponent)
                .Join(Entities, abilityComponent => abilityComponent.EntityId,
                    abilityEntity => abilityEntity.Id,
                    (_, abilityEntity) => abilityEntity);

            var effects = abilityEntities.Concat(abilityDefinitionEntities)
                .Join(EffectComponents,
                    abilityEntity => abilityEntity.Id, effectComponent => effectComponent.ContainingAbilityId,
                    (_, effectComponent) => effectComponent)
                .Where(effectComponent => effectComponent.AffectedEntityId == null);

            // TODO: Query nested ability definitions

            var allEntities = appliedEffects.Concat(effects)
                .Join(AggregateEntities,
                    effectComponent => effectComponent.EntityId, effectEntity => effectEntity.Id,
                    (_, effectEntity) => effectEntity)
                .Concat(affectableEntities)
                .Concat(abilityEntities)
                .Concat(abilityDefinitionEntities);

            allEntities.Load();
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

            Games.Remove(game);

            SaveChanges();
        }
    }
}

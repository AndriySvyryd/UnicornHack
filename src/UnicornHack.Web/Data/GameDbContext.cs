using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;

namespace UnicornHack.Data
{
    public class GameDbContext : DbContext
    {
        private Func<GameDbContext, string, IEnumerable<Player>> _loadPlayer;
        private Func<GameDbContext, int, string, byte, IEnumerable<Level>> _loadLevel;

        // ReSharper disable once SuggestBaseTypeForParameter
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<EntityKnowledge> EntitiesKnowledge { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<AbilityDefinition> AbilityDefinitions { get; set; }
        public DbSet<Ability> Abilities { get; set; }
        public DbSet<Trigger> Triggers { get; set; }
        public DbSet<Effect> Effects { get; set; }
        public DbSet<AppliedEffect> AppliedEffects { get; set; }
        public DbSet<SensoryEvent> SensoryEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Derive entity classes from NotificationEntity and
            // implement INotifyCollectionChanged on PriorityQueue and SortedListAdapter
            // modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);

            // TODO: Use non-shadow numerical discriminators
            // TODO: Add index on the discriminator

            modelBuilder.Entity<Branch>(eb =>
            {
                eb.HasKey(b => new {b.GameId, b.Name});
                eb.HasMany(b => b.Levels)
                    .WithOne(l => l.Branch)
                    .HasForeignKey(l => new {l.GameId, l.BranchName});
            });

            modelBuilder.Entity<Level>(eb =>
            {
                eb.Ignore(l => l.Players);
                eb.Ignore(l => l.IndexToPoint);
                eb.Ignore(l => l.PointToIndex);
                eb.Ignore(l => l.TerrainChanges);
                eb.Ignore(l => l.KnownTerrainChanges);
                eb.Ignore(l => l.WallNeighboursChanges);
                eb.Ignore(l => l.VisibleTerrainSnapshot);
                eb.Ignore(l => l.VisibleTerrainChanges);
                eb.Ignore(l => l.VisibleNeighboursChanged);
                eb.Ignore(l => l.FOV);
                eb.HasKey(l => new {l.GameId, l.BranchName, l.Depth});
                eb.HasMany(l => l.Connections)
                    .WithOne(s => s.Level)
                    .HasForeignKey(s => new {s.GameId, s.BranchName, s.LevelDepth});
                eb.HasMany(l => l.IncomingConnections)
                    .WithOne(s => s.TargetLevel)
                    .HasForeignKey(s => new {s.GameId, s.TargetBranchName, s.TargetLevelDepth});
                eb.HasMany(l => l.Rooms)
                    .WithOne(r => r.Level)
                    .HasForeignKey(r => new {r.GameId, r.BranchName, r.LevelDepth});
                eb.OwnsOne(g => g.GenerationRandom);
                eb.HasMany(l => l.ActorsKnowledge)
                    .WithOne(a => a.Level)
                    .HasForeignKey(a => new { a.GameId, a.BranchName, a.LevelDepth });
                eb.HasMany(l => l.ItemsKnowledge)
                    .WithOne(a => a.Level)
                    .HasForeignKey(a => new { a.GameId, a.BranchName, a.LevelDepth });
            });

            modelBuilder.Entity<Connection>(eb =>
            {
                eb.HasKey(c => new {c.GameId, c.Id});
                eb.HasOne(c => c.TargetBranch)
                    .WithMany()
                    .HasForeignKey(c => new {c.GameId, c.TargetBranchName});
            });

            modelBuilder.Entity<Room>(eb =>
            {
                eb.Ignore(l => l.BoundingRectangle);
                eb.Ignore(l => l.DoorwayPoints);
                eb.Ignore(l => l.InsidePoints);

                eb.Property<byte>("_x1");
                eb.Property<byte>("_x2");
                eb.Property<byte>("_y1");
                eb.Property<byte>("_y2");

                eb.HasKey(b => new {b.GameId, b.BranchName, b.LevelDepth, b.Id});
            });

            modelBuilder.Entity<EntityKnowledge>(eb =>
                eb.HasKey(e => new {e.GameId, e.Id}));

            modelBuilder.Entity<Entity>(eb =>
            {
                eb.Property("_referenceCount");
                eb.HasKey(e => new {e.GameId, e.Id});
                eb.HasMany(e => e.Abilities)
                    .WithOne(a => a.Entity)
                    .HasForeignKey(a => new {a.GameId, a.EntityId});
                eb.HasMany(e => e.ActiveEffects)
                    .WithOne(e => e.Entity)
                    .HasForeignKey(e => new {e.GameId, e.EntityId});
                eb.HasMany(e => e.Properties)
                    .WithOne(e => e.Entity)
                    .HasForeignKey(e => new {e.GameId, e.EntityId});
            });

            modelBuilder.Entity<Actor>(eb =>
            {
                eb.HasOne(a => a.Level)
                    .WithMany(l => l.Actors)
                    .HasForeignKey(a => new {a.GameId, a.BranchName, a.LevelDepth});
                eb.HasOne(p => p.NaturalWeapon)
                    .WithOne()
                    .HasForeignKey<Actor>(a => new { a.GameId, a.NaturalWeaponId });
                eb.HasOne(i => i.PlayerKnowledge)
                    .WithOne(k => k.Actor)
                    .HasForeignKey<ActorKnowledge>(k => new { k.GameId, k.Id })
                    .HasConstraintName("FK_EntitiesKnowledge_Entities_GameId_Id");
            });

            modelBuilder.Entity<Creature>();

            modelBuilder.Entity<Player>(pb =>
            {
                // TODO: Unify with Name after switching to TPT
                pb.HasIndex(a => a.PlayerName).IsUnique();
                pb.Ignore(p => p.XP);
                pb.Ignore(p => p.XPLevel);
                pb.Ignore(p => p.NextLevelXP);
                pb.Ignore(p => p.Races);
                pb.Ignore(l => l.SnapshotTick);
                pb.HasOne(p => p.DefaultAttack)
                    .WithOne()
                    .HasForeignKey<Player>(a => new {a.GameId, a.DefaultAttackId});
            });

            modelBuilder.Entity<Item>(eb =>
            {
                eb.HasOne(i => i.Level)
                    .WithMany(l => l.Items)
                    .HasForeignKey(i => new {i.GameId, i.BranchName, i.LevelDepth});
                eb.HasOne(i => i.Actor)
                    .WithMany(a => a.Inventory)
                    .HasForeignKey(i => new {i.GameId, i.ActorId});
                eb.HasOne(i => i.Container)
                    .WithMany(c => c.Items)
                    .HasForeignKey(i => new {i.GameId, i.ContainerId});
                eb.HasOne(i => i.PlayerKnowledge)
                    .WithOne(k => k.Item)
                    .HasForeignKey<ItemKnowledge>(k => new {k.GameId, k.Id})
                    .HasConstraintName("FK_EntitiesKnowledge_Entities_GameId_Id");
            });

            modelBuilder.Entity<ItemStack>();
            modelBuilder.Entity<Gold>();
            modelBuilder.Entity<Launcher>(eb =>
            {
                eb.HasOne(l => l.Projectile)
                    .WithOne()
                    .HasForeignKey<Item>(i => new {i.GameId, i.LauncherId});
            });

            modelBuilder.Entity<Skills>(eb =>
            {
                eb.HasKey(s => new {s.GameId, s.Id});
                eb.HasOne<Player>().WithOne(p => p.Skills)
                    .IsRequired().HasForeignKey<Skills>(s => new {s.GameId, s.Id});
            });

            modelBuilder.Entity<Property>(pb => pb.HasKey(p => new {p.GameId, p.EntityId, p.Name}));
            modelBuilder.Entity<CalculatedProperty<bool>>(pb =>
            {
                pb.Ignore(p => p.CurrentValue);
                pb.Property<bool>("_currentValue");
            });
            modelBuilder.Entity<CalculatedProperty<int>>(pb =>
            {
                pb.Ignore(p => p.CurrentValue);
                pb.Property<int>("_currentValue");
            });
            modelBuilder.Entity<DynamicProperty<bool>>(pb =>
            {
                pb.Ignore(p => p.CurrentValue);
                pb.Property<bool>("_currentValue");
            });
            modelBuilder.Entity<DynamicProperty<int>>(pb =>
            {
                pb.Ignore(p => p.CurrentValue);
                pb.Property<int>("_currentValue");
            });

            modelBuilder.Entity<Game>(eb =>
            {
                eb.Ignore(g => g.Services);
                eb.Ignore(g => g.Repository);
                eb.Ignore(g => g.Players);
                eb.Property(g => g.Id)
                    .ValueGeneratedOnAdd();

                eb.HasMany(g => g.Branches)
                    .WithOne(l => l.Game)
                    .HasForeignKey(l => l.GameId);
                eb.HasMany(g => g.Levels)
                    .WithOne(l => l.Game)
                    .HasForeignKey(l => l.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(g => g.Rooms)
                    .WithOne(l => l.Game)
                    .HasForeignKey(l => l.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(g => g.Entities)
                    .WithOne(a => a.Game)
                    .HasForeignKey(a => a.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(g => g.EntitiesKnowledge)
                    .WithOne(a => a.Game)
                    .HasForeignKey(a => a.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(g => g.Connections)
                    .WithOne(s => s.Game)
                    .HasForeignKey(s => s.GameId)
                    .OnDelete(DeleteBehavior.Cascade);
                eb.HasMany(g => g.Abilities)
                    .WithOne(a => a.Game)
                    .HasForeignKey(a => a.GameId);
                eb.HasMany(g => g.Triggers)
                    .WithOne(a => a.Game)
                    .HasForeignKey(a => a.GameId);
                eb.HasMany(g => g.Effects)
                    .WithOne(e => e.Game)
                    .HasForeignKey(e => e.GameId);
                eb.HasMany(g => g.AppliedEffects)
                    .WithOne(e => e.Game)
                    .HasForeignKey(e => e.GameId);
                eb.HasMany(g => g.SensoryEvents)
                    .WithOne(e => e.Game)
                    .HasForeignKey(e => e.GameId);
                eb.OwnsOne(g => g.Random);
            });

            modelBuilder.Entity<LogEntry>(eb =>
            {
                eb.Property(l => l.Message).IsRequired();
                eb.HasKey(l => new {l.GameId, l.PlayerId, l.Id});
                eb.HasOne(l => l.Player)
                    .WithMany(pc => pc.Log)
                    .HasForeignKey(l => new {l.GameId, l.PlayerId});
            });

            modelBuilder.Entity<PlayerCommand>(eb =>
            {
                eb.HasKey(l => new {l.GameId, l.PlayerId, l.Id});
                eb.HasOne(l => l.Player)
                    .WithMany(pc => pc.CommandHistory)
                    .HasForeignKey(l => new {l.GameId, l.PlayerId});
            });

            modelBuilder.Entity<QueuedCommand>(eb =>
            {
                eb.HasKey(l => new {l.GameId, l.PlayerId, l.Id});
                eb.HasOne(l => l.Player)
                    .WithMany(pc => pc.QueuedCommands)
                    .HasForeignKey(l => new {l.GameId, l.PlayerId});
            });

            modelBuilder.Entity<SensoryEvent>(eb =>
            {
                eb.Property("_referenceCount");
                eb.HasKey(l => new {l.GameId, l.SensorId, l.Id});
                eb.HasOne(e => e.Sensor)
                    .WithMany(a => a.SensedEvents)
                    .HasForeignKey(s => new {s.GameId, s.SensorId})
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ActorMoveEvent>();
            modelBuilder.Entity<AttackEvent>();
            modelBuilder.Entity<DeathEvent>();
            modelBuilder.Entity<ItemConsumptionEvent>();
            modelBuilder.Entity<ItemDropEvent>();
            modelBuilder.Entity<ItemPickUpEvent>();
            modelBuilder.Entity<ItemEquipmentEvent>();
            modelBuilder.Entity<ItemUnequipmentEvent>();

            modelBuilder.Entity<Ability>(eb =>
            {
                eb.Property("_referenceCount");
                eb.HasKey(a => new {a.GameId, a.Id});
                eb.HasMany(a => a.Effects)
                    .WithOne()
                    .HasForeignKey(e => new {e.GameId, e.DefiningAbilityId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(a => a.ActiveEffects)
                    .WithOne(e => e.SourceAbility)
                    .HasForeignKey(e => new {e.GameId, e.SourceAbilityId});
                eb.HasMany(a => a.Triggers)
                    .WithOne(t => t.Ability)
                    .HasForeignKey(t => new {t.GameId, t.AbilityId});
            });

            modelBuilder.Entity<Trigger>().HasKey(a => new {a.GameId, a.Id});
            modelBuilder.Entity<AbilityTrigger>().HasOne(e => e.AbilityToTrigger)
                .WithMany()
                .HasForeignKey(t => new {t.GameId, TriggeredAbilityId = t.AbilityToTriggerId})
                .OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<MeleeWeaponTrigger>()
                .HasOne(t => t.Weapon)
                .WithMany()
                .HasForeignKey(t => new {t.GameId, t.WeaponId})
                .OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<RangedWeaponTrigger>()
                .HasOne(t => t.Weapon)
                .WithMany()
                .HasForeignKey(t => new {t.GameId, t.WeaponId})
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<AbilityDefinition>(eb =>
            {
                eb.HasKey(a => new {a.GameId, a.Id});
                eb.HasMany(a => a.Triggers)
                    .WithOne()
                    .HasForeignKey(t => new {t.GameId, t.AbilityDefinitionId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(a => a.Effects)
                    .WithOne()
                    .HasForeignKey(e => new {e.GameId, e.AbilityDefinitionId})
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Effect>(eb => eb.HasKey(a => new {a.GameId, a.Id}));
            modelBuilder.Entity<AppliedEffect>(eb =>
            {
                eb.Property("_referenceCount");
                eb.HasKey(a => new {a.GameId, a.Id});
            });
            modelBuilder.Entity<AddAbility>(ab =>
            {
                ab.HasOne(e => e.Ability)
                    .WithOne()
                    .HasForeignKey<AbilityDefinition>(a => new {a.GameId, a.AddAbilityEffectId});
            });
            modelBuilder.Entity<AddedAbility>(ab =>
            {
                ab.HasOne(e => e.Ability)
                    .WithOne()
                    .HasForeignKey<Ability>(a => new {a.GameId, a.AddedAbilityEffectId});
            });
            modelBuilder.Entity<Blind>();
            modelBuilder.Entity<Blinded>();
            modelBuilder.Entity<Bind>();
            modelBuilder.Entity<Bound>();
            modelBuilder.Entity<Burn>();
            modelBuilder.Entity<Burned>();
            modelBuilder.Entity<ChangeProperty<bool>>();
            modelBuilder.Entity<ChangeProperty<int>>();
            modelBuilder.Entity<ChangedProperty>();
            modelBuilder.Entity<ChangedBoolProperty>();
            modelBuilder.Entity<ChangedIntProperty>();
            modelBuilder.Entity<ChangeRace>();
            modelBuilder.Entity<ChangedRace>();
            modelBuilder.Entity<ConferLycanthropy>();
            modelBuilder.Entity<LycanthropyConfered>();
            modelBuilder.Entity<Confuse>();
            modelBuilder.Entity<Confused>();
            modelBuilder.Entity<Corrode>();
            modelBuilder.Entity<Corroded>();
            modelBuilder.Entity<Cripple>();
            modelBuilder.Entity<Crippled>();
            modelBuilder.Entity<Curse>();
            modelBuilder.Entity<Cursed>();
            modelBuilder.Entity<Deafen>();
            modelBuilder.Entity<Deafened>();
            modelBuilder.Entity<Disarm>();
            modelBuilder.Entity<Disarmed>();
            modelBuilder.Entity<Disintegrate>();
            modelBuilder.Entity<Disintegrated>();
            modelBuilder.Entity<DrainEnergy>();
            modelBuilder.Entity<EnergyDrained>();
            modelBuilder.Entity<DrainLife>();
            modelBuilder.Entity<LifeDrained>();
            modelBuilder.Entity<Engulf>();
            modelBuilder.Entity<Engulfed>();
            modelBuilder.Entity<Envenom>();
            modelBuilder.Entity<Envenomed>();
            modelBuilder.Entity<Freeze>();
            modelBuilder.Entity<Frozen>();
            modelBuilder.Entity<GainXP>();
            modelBuilder.Entity<GainedXP>();
            modelBuilder.Entity<Heal>();
            modelBuilder.Entity<Healed>();
            modelBuilder.Entity<Infect>();
            modelBuilder.Entity<Infected>();
            modelBuilder.Entity<LevelTeleport>();
            modelBuilder.Entity<LevelTeleported>();
            modelBuilder.Entity<MagicalDamage>();
            modelBuilder.Entity<MagicallyDamaged>();
            modelBuilder.Entity<MeleeAttacked>()
                .HasOne(m => m.Weapon)
                .WithMany()
                .HasForeignKey(m => new {m.GameId, m.WeaponId})
                .OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<Paralyze>();
            modelBuilder.Entity<Paralyzed>();
            modelBuilder.Entity<PhysicalDamage>();
            modelBuilder.Entity<PhysicallyDamaged>();
            modelBuilder.Entity<Poison>();
            modelBuilder.Entity<Poisoned>();
            modelBuilder.Entity<RangeAttacked>()
                .HasOne(m => m.Weapon)
                .WithMany()
                .HasForeignKey(m => new {m.GameId, m.WeaponId})
                .OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<ScriptedEffect>();
            modelBuilder.Entity<Sedate>();
            modelBuilder.Entity<Sedated>();
            modelBuilder.Entity<Shock>();
            modelBuilder.Entity<Shocked>();
            modelBuilder.Entity<Slime>();
            modelBuilder.Entity<Slimed>();
            modelBuilder.Entity<Slow>();
            modelBuilder.Entity<Slowed>();
            modelBuilder.Entity<Soak>();
            modelBuilder.Entity<Soaked>();
            modelBuilder.Entity<StealGold>();
            modelBuilder.Entity<GoldStolen>();
            modelBuilder.Entity<StealItem>();
            modelBuilder.Entity<ItemStolen>();
            modelBuilder.Entity<Stick>();
            modelBuilder.Entity<Stuck>();
            modelBuilder.Entity<Stone>();
            modelBuilder.Entity<Stoned>();
            modelBuilder.Entity<Stun>();
            modelBuilder.Entity<Stunned>();
            modelBuilder.Entity<Suffocate>();
            modelBuilder.Entity<Suffocated>();
            modelBuilder.Entity<Teleport>();
            modelBuilder.Entity<Teleported>();

            modelBuilder.Entity<ActorMoveEvent>(eb =>
            {
                eb.HasOne(e => e.Mover)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.MoverId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(e => e.Movee)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.MoveeId})
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<AttackEvent>(eb =>
            {
                eb.HasOne(e => e.Attacker)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.AttackerId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(e => e.Victim)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.VictimId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(e => e.AppliedEffects)
                    .WithOne()
                    .HasForeignKey(e => new {e.GameId, e.SensorId, e.AttackEventId})
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<DeathEvent>(eb =>
            {
                eb.HasOne(e => e.Deceased)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.DeceasedId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(e => e.Corpse)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.CorpseId})
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ItemConsumptionEvent>(eb =>
            {
                eb.HasOne(e => e.Consumer)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ConsumerId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId})
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ItemDropEvent>(eb =>
            {
                eb.HasOne(e => e.Dropper)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.DropperId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId})
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ItemPickUpEvent>(eb =>
            {
                eb.HasOne(e => e.Picker)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.PickerId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId})
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ItemEquipmentEvent>(eb =>
            {
                eb.HasOne(e => e.Equipper)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.EquipperId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId})
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ItemUnequipmentEvent>(eb =>
            {
                eb.HasOne(e => e.Unequipper)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.UnequipperId})
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId})
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public Player LoadPlayer(string name)
        {
            if (_loadPlayer == null)
            {
                _loadPlayer = EF.CompileQuery((GameDbContext context, string playerName) =>
                    context.Players
                        .Include(c => c.Game.Random)
                        .Include(c => c.Skills)
                        .Include(c => c.QueuedCommands)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ActorMoveEvent, Actor>(e => e.Mover)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ActorMoveEvent, Actor>(e => e.Movee)
                        .Include(c => c.SensedEvents)
                        .ThenInclude(e => ((AttackEvent)e).AppliedEffects)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, AttackEvent, Actor>(e => e.Attacker)
                        .Include(c => c.SensedEvents)
                        .ThenInclude(e => ((AttackEvent)e).Victim)
                        .Include(c => c.SensedEvents)
                        .ThenInclude(e => ((DeathEvent)e).Deceased)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, DeathEvent, Item>(e => e.Corpse)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemConsumptionEvent, Actor>(e => e.Consumer)
                        .Include(c => c.SensedEvents)
                        .ThenInclude(e => ((ItemConsumptionEvent)e).Item)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemDropEvent, Actor>(e => e.Dropper)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemDropEvent, Item>(e => e.Item)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemPickUpEvent, Actor>(e => e.Picker)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemPickUpEvent, Item>(e => e.Item)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemEquipmentEvent, Actor>(e => e.Equipper)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemEquipmentEvent, Item>(e => e.Item)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemUnequipmentEvent, Actor>(e => e.Unequipper)
                        //.Include(c => c.SensedEvents).ThenInclude<Player, SensoryEvent, ItemUnequipmentEvent, Item>(e => e.Item)
                        .Include(c => c.Level)
                        .Where(c => c.PlayerName == playerName));
            }

            var player = _loadPlayer(this, name).First();

            LogEntries
                .Where(e => e.GameId == player.GameId && e.PlayerId == player.Id)
                .OrderByDescending(e => e.Tick)
                .ThenByDescending(e => e.Id)
                .Take(10)
                .Load();

            return player;
        }

        public Level LoadLevel(int gameId, string branchName, byte depth)
        {
            if (_loadLevel == null)
            {
                _loadLevel = EF.CompileQuery((GameDbContext context, int id, string name, byte d) =>
                    context.Levels
                        .Include(l => l.Items).ThenInclude(i => i.Abilities).ThenInclude(a => a.Triggers)
                        .Include(l => l.Items).ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                        .Include(l => l.Items).ThenInclude(i => i.ActiveEffects).ThenInclude(e => e.SourceAbility)
                        .Include(l => l.Items).ThenInclude(i => i.Properties)
                        .Include(l => l.Items).ThenInclude(c => ((Container)c).Items)
                        .ThenInclude(i => i.ActiveEffects).ThenInclude(e => e.SourceAbility)
                        .Include(l => l.Items).ThenInclude(c => ((Container)c).Items)
                        .ThenInclude(i => i.Properties)
                        .Include(l => l.Items).ThenInclude(c => ((Container)c).Items)
                        .ThenInclude(i => i.Abilities).ThenInclude(a => a.Triggers)
                        .Include(l => l.Items).ThenInclude(c => ((Container)c).Items)
                        .ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                        .Include(l => l.Items).ThenInclude(l => ((Launcher)l).Projectile)
                        .ThenInclude(i => i.Properties)
                        .Include(l => l.ItemsKnowledge)
                        .Include(l => l.Actors).ThenInclude(a => a.NaturalWeapon)
                        .ThenInclude(i => i.Abilities).ThenInclude(a => a.Triggers)
                        .Include(l => l.Actors).ThenInclude(a => a.NaturalWeapon)
                        .ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                        .Include(l => l.Actors).ThenInclude(a => a.NaturalWeapon)
                        .ThenInclude(i => i.ActiveEffects).ThenInclude(e => e.SourceAbility)
                        .Include(l => l.Actors).ThenInclude(a => a.NaturalWeapon).ThenInclude(i => i.Properties)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(i => i.Abilities)
                        .ThenInclude(a => a.Triggers)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(i => i.Abilities)
                        .ThenInclude(a => a.Effects)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(i => i.ActiveEffects)
                        .ThenInclude(e => e.SourceAbility)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(c => ((Container)c).Items)
                        .ThenInclude(i => i.Abilities).ThenInclude(a => a.Triggers)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(c => ((Container)c).Items)
                        .ThenInclude(i => i.Abilities).ThenInclude(a => a.Effects)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(c => ((Container)c).Items)
                        .ThenInclude(i => i.ActiveEffects).ThenInclude(e => e.SourceAbility)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(c => ((Container)c).Items)
                        .ThenInclude(i => i.Properties)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory)
                        .ThenInclude(l => ((Launcher)l).Projectile).ThenInclude(i => i.Properties)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(l => ((Launcher)l).Projectile)
                        .ThenInclude(a => a.Abilities).ThenInclude(a => a.Triggers)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(l => ((Launcher)l).Projectile)
                        .ThenInclude(a => a.Abilities).ThenInclude(a => a.Effects)
                        .Include(l => l.Actors).ThenInclude(a => a.Inventory).ThenInclude(i => i.Properties)
                        .Include(l => l.Actors).ThenInclude(a => a.Abilities).ThenInclude(a => a.Triggers)
                        .Include(l => l.Actors).ThenInclude(a => a.Abilities).ThenInclude(a => a.Effects)
                        .Include(l => l.Actors).ThenInclude(a => a.Abilities).ThenInclude(a => a.ActiveEffects)
                        .Include(l => l.Actors).ThenInclude(a => a.ActiveEffects).ThenInclude(e => e.SourceAbility)
                        .Include(l => l.Actors).ThenInclude(a => a.Properties)
                        .Include(l => l.ActorsKnowledge)
                        .Include(l => l.Connections)
                        .Include(l => l.IncomingConnections)
                        .Include(l => l.Rooms)
                        .Include(l => l.Branch)
                        .Where(l => l.GameId == id && l.BranchName == name && l.Depth == d));
            }

            var level = _loadLevel(this, gameId, branchName, depth).First();

            var addedAbilities = Set<AddedAbility>().Local.Select(c => c.Id).ToList();
            if (addedAbilities.Count > 0)
            {
                Set<AddedAbility>()
                    .Include(a => a.Ability).ThenInclude(a => a.Triggers)
                    .Include(a => a.Ability).ThenInclude(a => a.Effects)
                    .Where(a => a.GameId == gameId && addedAbilities.Contains(a.Id))
                    .Load();
            }

            var addAbilities = Set<AddAbility>().Local.Select(c => c.Id).ToList();
            if (addAbilities.Count > 0)
            {
                Set<AddAbility>()
                    .Include(a => a.Ability).ThenInclude(a => a.Triggers)
                    .Include(a => a.Ability).ThenInclude(a => a.Effects)
                    .Where(a => a.GameId == gameId && addAbilities.Contains(a.Id))
                    .Load();
            }

            return level;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation.Map;
using UnicornHack.Utils;

namespace UnicornHack.Models
{
    public class GameDbContext : DbContext
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Player> Characters { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<Ability> Abilities { get; set; }
        public DbSet<Effect> Effects { get; set; }
        public DbSet<SensoryEvent> SensoryEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                eb.Property(l => l.Terrain).IsRequired();
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

                eb.HasKey(b => new { b.GameId, b.BranchName, b.LevelDepth, b.Id });
            });

            modelBuilder.Entity<Actor>(eb =>
            {
                eb.Property("_referenceCount");
                eb.HasKey(a => new {a.GameId, a.Id});
                eb.HasIndex(a => a.Name).IsUnique();
                eb.HasOne(a => a.Level)
                    .WithMany(l => l.Actors)
                    .HasForeignKey(a => new {a.GameId, a.BranchName, a.LevelDepth});
                eb.HasMany(a => a.Abilities)
                    .WithOne()
                    .HasForeignKey(nameof(Ability.GameId), "ActorId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Creature>(cb => {});

            modelBuilder.Entity<Player>(pb =>
            {
                pb.Ignore(p => p.XP);
                pb.Ignore(p => p.XPLevel);
                pb.Ignore(p => p.NextLevelXP);
            });

            modelBuilder.Entity<PlayerRace>(rb =>
            {
                rb.HasKey(r => new {r.GameId, r.PlayerId, r.Id});
                rb.HasOne(r => r.Player)
                    .WithMany(p => p.Races)
                    .IsRequired()
                    .HasForeignKey(r => new {r.GameId, r.PlayerId});
                rb.HasMany(i => i.Abilities)
                    .WithOne()
                    .HasForeignKey(nameof(Ability.GameId), "ActivePlayerId", "ActivePlayerRaceId");
            });

            // TODO: Owned type
            modelBuilder.Entity<Skills>(eb =>
            {
                eb.Property<int>("GameId");
                eb.Property<int>("Id");
                eb.HasKey("GameId", "Id");
                eb.HasOne<Player>().WithOne(p => p.Skills)
                    .IsRequired().HasForeignKey<Skills>("GameId", "Id");
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
                eb.HasMany(g => g.Actors)
                    .WithOne(a => a.Game)
                    .HasForeignKey(a => a.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(g => g.Items)
                    .WithOne(i => i.Game)
                    .HasForeignKey(i => i.GameId);
                eb.HasMany(g => g.Connections)
                    .WithOne(s => s.Game)
                    .HasForeignKey(s => s.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(g => g.Abilities)
                    .WithOne(a => a.Game)
                    .HasForeignKey(a => a.GameId);
                eb.HasMany(g => g.Effects)
                    .WithOne(e => e.Game)
                    .HasForeignKey(e => e.GameId);
                eb.HasMany(g => g.SensoryEvents)
                    .WithOne(e => e.Game)
                    .HasForeignKey(e => e.GameId);
            });

            // TODO: Owned type
            modelBuilder.Entity<SimpleRandom>(eb =>
            {
                eb.HasOne<Game>()
                    .WithOne(g => g.Random)
                    .HasForeignKey<Game>("RandomId");
                eb.HasOne<Level>()
                    .WithOne(l => l.GenerationRandom)
                    .HasForeignKey<Level>("RandomId");
            });

            modelBuilder.Entity<Item>(eb =>
            {
                eb.Ignore(i => i.BaseItem);
                eb.Ignore(i => i.SimpleProperties);
                eb.Ignore(i => i.ValuedProperties);
                eb.Property("_referenceCount");
                eb.HasKey(i => new {i.GameId, i.Id});
                eb.HasOne(i => i.Level)
                    .WithMany(l => l.Items)
                    .HasForeignKey(i => new {i.GameId, i.BranchName, i.LevelDepth});
                eb.HasOne(i => i.Actor)
                    .WithMany(a => a.Inventory)
                    .HasForeignKey(i => new {i.GameId, i.ActorId});
                eb.HasOne(i => i.Container)
                    .WithMany(c => c.Items)
                    .HasForeignKey(i => new {i.GameId, i.ContainerId});
                eb.HasMany(i => i.Abilities)
                    .WithOne()
                    .HasForeignKey(a => new { a.GameId, a.ItemId });
            });

            modelBuilder.Entity<ItemStack>();
            modelBuilder.Entity<Gold>();

            modelBuilder.Entity<Connection>(eb =>
            {
                eb.HasKey(c => new {c.GameId, c.Id});
                eb.HasOne(c => c.TargetBranch)
                    .WithMany()
                    .HasForeignKey(c => new {c.GameId, c.TargetBranchName});
            });

            modelBuilder.Entity<LogEntry>(eb =>
            {
                eb.Property(l => l.Message).IsRequired();
                eb.HasKey(l => new {l.GameId, l.PlayerId, l.Id});
                eb.HasOne(l => l.Player)
                    .WithMany(pc => pc.Log)
                    .HasForeignKey(l => new {l.GameId, l.PlayerId});
            });

            modelBuilder.Entity<SensoryEvent>(eb =>
            {
                eb.Property("_referenceCount");
                eb.HasKey(l => new {l.GameId, l.SensorId, l.Id});
                eb.HasOne(e => e.Sensor)
                    .WithMany(a => a.SensedEvents)
                    .HasForeignKey(s => new {s.GameId, s.SensorId});
                eb.ToTable(name: "SensoryEvents");
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
                    .HasForeignKey(nameof(Effect.GameId), "AbilityId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Effect>(eb =>
            {
                eb.Property("_referenceCount");
                eb.HasKey(a => new {a.GameId, a.Id});
            });
            modelBuilder.Entity<AcidDamage>();
            modelBuilder.Entity<AddAbility>();
            modelBuilder.Entity<Blind>();
            modelBuilder.Entity<Bind>();
            modelBuilder.Entity<ChangeRace>();
            modelBuilder.Entity<ChangeProperty<bool>>();
            modelBuilder.Entity<ChangeProperty<int>>();
            modelBuilder.Entity<ChangeProperty<Size>>();
            modelBuilder.Entity<ColdDamage>();
            modelBuilder.Entity<ConferLycanthropy>();
            modelBuilder.Entity<Confuse>();
            modelBuilder.Entity<Cripple>();
            modelBuilder.Entity<Curse>();
            modelBuilder.Entity<Deafen>();
            modelBuilder.Entity<Disarm>();
            modelBuilder.Entity<Disenchant>();
            modelBuilder.Entity<Disintegrate>();
            modelBuilder.Entity<DrainEnergy>();
            modelBuilder.Entity<DrainLife>();
            modelBuilder.Entity<ElectricityDamage>();
            modelBuilder.Entity<Engulf>();
            modelBuilder.Entity<FireDamage>();
            modelBuilder.Entity<Hallucinate>();
            modelBuilder.Entity<Heal>();
            modelBuilder.Entity<Infect>();
            modelBuilder.Entity<LevelTeleport>();
            modelBuilder.Entity<MagicalDamage>();
            modelBuilder.Entity<MeleeAttack>()
                .HasOne(m => m.Weapon)
                .WithMany()
                // TODO: Use GameId, #7181
                .HasForeignKey("GameId2", nameof(MeleeAttack.WeaponId));
            modelBuilder.Entity<Paralyze>();
            modelBuilder.Entity<PhysicalDamage>();
            modelBuilder.Entity<PoisonDamage>();
            modelBuilder.Entity<Polymorph>();
            modelBuilder.Entity<ScriptedEffect>();
            modelBuilder.Entity<Seduce>();
            modelBuilder.Entity<Sleep>();
            modelBuilder.Entity<Slime>();
            modelBuilder.Entity<Slow>();
            modelBuilder.Entity<StealGold>();
            modelBuilder.Entity<StealItem>();
            modelBuilder.Entity<Stick>();
            modelBuilder.Entity<Stone>();
            modelBuilder.Entity<Stun>();
            modelBuilder.Entity<Suffocate>();
            modelBuilder.Entity<Teleport>();
            modelBuilder.Entity<VenomDamage>();
            modelBuilder.Entity<WaterDamage>();
        }

        private void OnModelCreatingToDo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActorMoveEvent>(eb =>
            {
                eb.HasOne(e => e.Mover)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.MoverId});
                eb.HasOne(e => e.Movee)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.MoverId});
            });
            modelBuilder.Entity<AttackEvent>(eb =>
            {
                eb.HasOne(e => e.Attacker)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.AttackerId});
                eb.HasOne(e => e.Victim)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.VictimId});
                eb.HasOne(e => e.Ability)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.AbilityId});
            });
            modelBuilder.Entity<DeathEvent>(eb =>
            {
                eb.HasOne(e => e.Deceased)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.DeceasedId});
                eb.HasOne(e => e.Corpse)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.CorpseId});
            });
            modelBuilder.Entity<ItemConsumptionEvent>(eb =>
            {
                eb.HasOne(e => e.Consumer)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ConsumerId});
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId});
            });
            modelBuilder.Entity<ItemDropEvent>(eb =>
            {
                eb.HasOne(e => e.Dropper)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.DropperId});
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId});
            });
            modelBuilder.Entity<ItemPickUpEvent>(eb =>
            {
                eb.HasOne(e => e.Picker)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.PickerId});
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId});
            });
            modelBuilder.Entity<ItemEquipmentEvent>(eb =>
            {
                eb.HasOne(e => e.Equipper)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.EquipperId});
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId});
            });
            modelBuilder.Entity<ItemUnequipmentEvent>(eb =>
            {
                eb.HasOne(e => e.Unequipper)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.UnequipperId});
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasForeignKey(e => new {e.GameId, e.ItemId});
            });
        }
    }
}
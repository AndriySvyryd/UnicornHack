using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using UnicornHack.Effects;
using UnicornHack.Events;

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
        public DbSet<Level> Levels { get; set; }
        public DbSet<Stairs> Stairs { get; set; }
        public DbSet<Player> Characters { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<Ability> Abilities { get; set; }
        public DbSet<Effect> Effects { get; set; }
        public DbSet<SensoryEvent> SensoryEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Level>(eb =>
            {
                eb.Ignore(l => l.Players);
                eb.Property(l => l.Layout).IsRequired();
                eb.Property(l => l.Name).IsRequired();
                eb.HasKey(l => new {l.GameId, l.Id});
                eb.HasMany(l => l.UpStairs)
                    .WithOne(s => s.Down)
                    .HasForeignKey(s => new {s.GameId, s.DownId});
                eb.HasMany(l => l.DownStairs)
                    .WithOne(s => s.Up)
                    .HasForeignKey(s => new {s.GameId, s.UpId});
            });

            modelBuilder.Entity<Actor>(eb =>
            {
                eb.Ignore(a => a.BaseActor);
                eb.Ignore(a => a.SimpleProperties);
                eb.Ignore(a => a.ValuedProperties);
                eb.Ignore(a => a.MovementDelay);
                eb.Property("_referenceCount");
                eb.HasKey(a => new {a.GameId, a.Id});
                eb.HasIndex(a => a.Name).IsUnique();
                eb.HasOne(a => a.Level)
                    .WithMany(l => l.Actors)
                    .HasForeignKey(a => new {a.GameId, a.LevelId});
                eb.HasMany(a => a.Abilities)
                    .WithOne()
                    .HasForeignKey(nameof(Ability.GameId), "ActorId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Player>();
            modelBuilder.Entity<Creature>();
            // TODO: Complex type
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
                eb.Ignore(g => g.Delete);
                eb.Ignore(g => g.Players);
                eb.Property(g => g.Id)
                    .ValueGeneratedOnAdd();
                eb.HasMany(g => g.Levels)
                    .WithOne(l => l.Game)
                    .HasForeignKey(l => l.GameId);
                eb.HasMany(g => g.Actors)
                    .WithOne(a => a.Game)
                    .HasForeignKey(a => a.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasMany(g => g.Items)
                    .WithOne(i => i.Game)
                    .HasForeignKey(i => i.GameId);
                eb.HasMany(g => g.Stairs)
                    .WithOne(s => s.Game)
                    .HasForeignKey(s => s.GameId);
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

            modelBuilder.Entity<Item>(eb =>
            {
                eb.Ignore(i => i.SimpleProperties);
                eb.Ignore(i => i.ValuedProperties);
                eb.Property("_referenceCount");
                eb.HasKey(i => new {i.GameId, i.Id});
                eb.HasOne(i => i.Level)
                    .WithMany(l => l.Items)
                    .HasForeignKey(i => new {i.GameId, i.LevelId});
                eb.HasOne(i => i.Actor)
                    .WithMany(a => a.Inventory)
                    .HasForeignKey(i => new {i.GameId, i.ActorId});
                eb.HasOne(i => i.Container)
                    .WithMany(c => c.Items)
                    .HasForeignKey(i => new {i.GameId, i.ContainerId});
                eb.HasMany(i => i.Abilities)
                    .WithOne()
                    .HasForeignKey(nameof(Ability.GameId), "ItemId");
            });
            modelBuilder.Entity<ItemStack>();
            modelBuilder.Entity<Gold>();

            modelBuilder.Entity<Stairs>(eb => { eb.HasKey(s => new {s.GameId, s.Id}); });

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
            modelBuilder.Entity<ChangeSimpleProperty>();
            modelBuilder.Entity<ChangeValuedProperty>();
            modelBuilder.Entity<ColdDamage>();
            modelBuilder.Entity<ConferLycanthropy>();
            modelBuilder.Entity<Confuse>();
            modelBuilder.Entity<Cripple>();
            modelBuilder.Entity<Curse>();
            modelBuilder.Entity<Deafen>();
            modelBuilder.Entity<Disarm>();
            modelBuilder.Entity<Disenchant>();
            modelBuilder.Entity<Disintegrate>();
            modelBuilder.Entity<DrainConstitution>();
            modelBuilder.Entity<DrainDexterity>();
            modelBuilder.Entity<DrainEnergy>();
            modelBuilder.Entity<DrainIntelligence>();
            modelBuilder.Entity<DrainLife>();
            modelBuilder.Entity<DrainSpeed>();
            modelBuilder.Entity<DrainStrength>();
            modelBuilder.Entity<DrainWillpower>();
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
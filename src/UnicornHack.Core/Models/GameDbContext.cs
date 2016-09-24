using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using UnicornHack.Models.GameState;
using UnicornHack.Models.GameState.Events;

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
        public DbSet<PlayerCharacter> Characters { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Level>(eb =>
            {
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
                eb.Property<string>(propertyName: "OriginalVariant");
                eb.Property<string>(propertyName: "PolymorphedVariant");
                eb.HasKey(a => new {a.GameId, a.Id});
                eb.HasOne(a => a.Level)
                    .WithMany(l => l.Actors)
                    .HasForeignKey(nameof(Actor.GameId), "LevelId")
                    .IsRequired();
            });

            modelBuilder.Entity<PlayerCharacter>();
            modelBuilder.Entity<Creature>();

            modelBuilder.Entity<Game>(eb =>
            {
                eb.Property(g => g.Id)
                    .ValueGeneratedOnAdd();
                eb.HasMany(g => g.Levels)
                    .WithOne(l => l.Game)
                    .HasForeignKey(l => l.GameId);
                eb.HasMany(g => g.Actors)
                    .WithOne(a => a.Game)
                    .HasForeignKey(a => a.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(g => g.ActingActor)
                    .WithOne()
                    .HasForeignKey<Game>(g => new {g.Id, g.ActingActorId})
                    .IsRequired(required: false);
                eb.HasMany(g => g.Items)
                    .WithOne(i => i.Game)
                    .HasForeignKey(i => i.GameId);
                eb.HasMany(g => g.Stairs)
                    .WithOne(s => s.Game)
                    .HasForeignKey(s => s.GameId);
            });

            modelBuilder.Entity<Item>(eb =>
            {
                eb.HasKey(i => new {i.GameId, i.Id});
                eb.HasOne(i => i.Level)
                    .WithMany(l => l.Items)
                    .HasForeignKey(nameof(Item.GameId), "LevelId");
            });
            modelBuilder.Entity<Weapon>();
            modelBuilder.Entity<Armor>();
            modelBuilder.Entity<StackableItem>();

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
                eb.HasKey(l => new {l.GameId, l.SensorId, l.Id});
                eb.HasOne(e => (PlayerCharacter)e.Sensor)
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
        }
    }
}
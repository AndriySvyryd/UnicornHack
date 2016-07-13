using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using UnicornHack.Models.GameState;
using UnicornHack.Models.GameState.Events;

namespace UnicornHack.Data
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
                eb.HasMany(l => l.UpStairs).WithOne(s => s.Down);
                eb.HasMany(l => l.DownStairs).WithOne(s => s.Up);
            });

            modelBuilder.Entity<Weapon>();
            modelBuilder.Entity<Armor>();
            modelBuilder.Entity<StackableItem>();

            modelBuilder.Entity<Actor>(eb =>
            {
                eb.Property<string>(propertyName: "OriginalVariant");
                eb.Property<string>(propertyName: "PolymorphedVariant");
                eb.HasOne(a => a.Level).WithMany(l => l.Actors).IsRequired();
            });

            modelBuilder.Entity<PlayerCharacter>();
            modelBuilder.Entity<Monster>();
            modelBuilder.Entity<LogEntry>(eb =>
            {
                eb.Property(l => l.Message).IsRequired();
                eb.HasKey(l => new {l.Id, l.PlayerId});
            });

            modelBuilder.Entity<Game>(eb =>
            {
                eb.HasMany(g => g.Actors).WithOne(a => a.Game).HasForeignKey(a => a.GameId).IsRequired().OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(g => g.ActingActor).WithOne().HasPrincipalKey<Actor>(a => a.Id).IsRequired(required: false);
                eb.HasMany(g => g.Levels).WithOne(l => l.Game).IsRequired();
                eb.HasMany(g => g.Items).WithOne(i => i.Game).IsRequired();
                eb.HasMany(g => g.Stairs).WithOne(s => s.Game).IsRequired();
            });

            modelBuilder.Entity<SensoryEvent>(eb =>
            {
                eb.HasKey(l => new {l.Id, l.SensorId});
                eb.ToTable(name: "SensoryEvents");
            });
            modelBuilder.Entity<ActorMoveEvent>();
            modelBuilder.Entity<AttackEvent>();
            modelBuilder.Entity<DeathEvent>();
            modelBuilder.Entity<ItemConsumptionEvent>();
            modelBuilder.Entity<ItemDropEvent>();
            modelBuilder.Entity<ItemPickUpEvent>(eb =>
            {
                // TODO: #5769
                eb.HasOne(e => e.Item)
                    .WithMany()
                    .HasConstraintName(name: "FK_ItemPickUpEvent_Items_ItemId");
            });
        }
    }
}
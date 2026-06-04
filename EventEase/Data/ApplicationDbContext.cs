using EventEase.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Normal constructor used by dependency injection in the running app
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Design-time constructor for migrations (only used when running dotnet ef commands)
        public ApplicationDbContext() : base(GetDesignTimeOptions())
        {
        }

        private static DbContextOptions<ApplicationDbContext> GetDesignTimeOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            // Use your local database for generating migrations
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EventEaseDB;Trusted_Connection=True;TrustServerCertificate=True");
            return optionsBuilder.Options;
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed predefined event type categories
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, Name = "Conference" },
                new EventType { EventTypeId = 2, Name = "Workshop" },
                new EventType { EventTypeId = 3, Name = "Wedding" },
                new EventType { EventTypeId = 4, Name = "Party" },
                new EventType { EventTypeId = 5, Name = "Seminar" },
                new EventType { EventTypeId = 6, Name = "Concert" },
                new EventType { EventTypeId = 7, Name = "Exhibition" },
                new EventType { EventTypeId = 8, Name = "Other" }
            );

            // Existing relationships
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.EventId)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithOne(e => e.Booking)
                .HasForeignKey<Booking>(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
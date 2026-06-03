using EventEase.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

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
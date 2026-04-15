using EventEase.Models;
using Microsoft.EntityFrameworkCore;
namespace EventEase.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>().HasIndex(b => b.EventId).IsUnique();
        modelBuilder.Entity<Booking>().HasOne(b => b.Venue).WithMany(v => v.Bookings).HasForeignKey(b => b.VenueId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Booking>().HasOne(b => b.Event).WithOne(e => e.Booking).HasForeignKey<Booking>(b => b.EventId).OnDelete(DeleteBehavior.Restrict);
    }
}
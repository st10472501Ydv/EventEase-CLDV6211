using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Location { get; set; }

        [Range(1, 10000)]
        public int Capacity { get; set; }

        public string? ImageUrl { get; set; }   // now a blob file name, nullable

        public string? Description { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}
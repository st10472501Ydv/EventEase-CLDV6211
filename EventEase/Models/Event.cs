using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        [DateGreaterThan(nameof(StartDate), ErrorMessage = "End date must be later than start date.")]
        public DateTime EndDate { get; set; }

        public string? OrganizerName { get; set; }
        public string? OrganizerEmail { get; set; }

        // Navigation
        public Booking? Booking { get; set; }

        public int? EventTypeId { get; set; } //classify event type
        public EventType? EventType { get; set; }
    }
}
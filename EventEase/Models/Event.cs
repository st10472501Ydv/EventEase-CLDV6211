using System.ComponentModel.DataAnnotations;

namespace EventEase.Models;

public class Event
{
    public int EventId { get; set; }
    [Required] public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerEmail { get; set; }
    public Booking? Booking { get; set; }
}
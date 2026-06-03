using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public ICollection<Event>? Events { get; set; }
    }
}
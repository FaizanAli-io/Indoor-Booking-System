using System.ComponentModel.DataAnnotations;

namespace IndoorBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required, MinLength(3)]
        public string Title { get; set; } = string.Empty;

        [Required, MinLength(10)]
        public string Description { get; set; } = string.Empty;

        public string[] MediaUrls { get; set; } = Array.Empty<string>();

        [Required, MinLength(3)]
        public string Location { get; set; } = string.Empty;

        [Required, Range(1, 24)]
        public int DurationHours { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

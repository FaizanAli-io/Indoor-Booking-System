using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndoorBookingSystem.Models
{
    public class Booking
    {
        [Key]
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PartitionKey { get; set; } = "Booking";

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, MinLength(3)]
        public string Title { get; set; } = string.Empty;

        [Required, MinLength(10)]
        public string Description { get; set; } = string.Empty;

        public string[] MediaUrls { get; set; } = Array.Empty<string>();

        [Required, MinLength(3)]
        public string Location { get; set; } = string.Empty;

        [Required, Range(1, 24)]
        public int DurationHours { get; set; }

        [Required, Range(0, long.MaxValue)]
        public long PriceCents { get; set; }

        [NotMapped]
        public decimal Price
        {
            get => PriceCents / 100m;
            set => PriceCents = (long)Math.Round(value * 100m);
        }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

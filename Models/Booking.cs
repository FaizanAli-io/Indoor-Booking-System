using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndoorBookingSystem.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected,
        Cancelled
    }

    public class Booking
    {
        [Key]
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PartitionKey { get; set; } = "Booking";

        [Required]
        public string IndoorId { get; set; } = string.Empty;

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public int SlotId { get; set; } // 0-23 (hour of day)

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public string? ConfirmedByAdminId { get; set; }

        // Store the actual price at the time of booking
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

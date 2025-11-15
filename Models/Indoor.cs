using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndoorBookingSystem.Models
{
    public class Indoor
    {
        [Key]
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PartitionKey { get; set; } = "Indoor";

        [Required]
        public string AdminId { get; set; } = string.Empty;

        [Required, MinLength(3)]
        public string Name { get; set; } = string.Empty;

        [Required, MinLength(10)]
        public string Description { get; set; } = string.Empty;

        public string[] MediaUrls { get; set; } = Array.Empty<string>();

        [Required, MinLength(3)]
        public string Location { get; set; } = string.Empty;

        [Required, Range(0, long.MaxValue)]
        public long PricePerHourCents { get; set; }

        [NotMapped]
        public decimal PricePerHour
        {
            get => PricePerHourCents / 100m;
            set => PricePerHourCents = (long)Math.Round(value * 100m);
        }

        // Dynamic pricing rules
        public List<PricingRule> PricingRules { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Helper method to get price for a specific date and slot ID
        public decimal GetPriceForSlot(DateTime date, int slotId)
        {
            if (PricingRules == null || PricingRules.Count == 0)
                return PricePerHour;

            var dayName = date.DayOfWeek.ToString();

            // Find matching pricing rule - check if slot ID is in the rule's slot array
            var matchingRule = PricingRules.FirstOrDefault(rule =>
                rule.DayNames != null && 
                rule.DayNames.Contains(dayName) &&
                rule.SlotIds != null &&
                rule.SlotIds.Contains(slotId)
            );

            return matchingRule?.Price ?? PricePerHour;
        }
    }
}

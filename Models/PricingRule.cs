using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndoorBookingSystem.Models
{
    public class PricingRule
    {
        // Store as string array for Cosmos DB compatibility
        [Required]
        public string[] DayNames { get; set; } = Array.Empty<string>();

        [NotMapped]
        public List<DayOfWeek> ApplicableDays
        {
            get => DayNames.Select(d => Enum.Parse<DayOfWeek>(d)).ToList();
            set => DayNames = value.Select(d => d.ToString()).ToArray();
        }

        // NEW: Store slot IDs instead of time ranges
        [Required]
        public int[] SlotIds { get; set; } = Array.Empty<int>();

        [Required, Range(0, long.MaxValue)]
        public long PriceCents { get; set; }

        [NotMapped]
        public decimal Price
        {
            get => PriceCents / 100m;
            set => PriceCents = (long)Math.Round(value * 100m);
        }

        public string DisplayText
        {
            get
            {
                if (DayNames == null || DayNames.Length == 0 || SlotIds == null || SlotIds.Length == 0)
                    return $"PKR {Price:N0}";
                
                var days = string.Join(", ", DayNames.Select(d => d.Substring(0, 3)));
                var slotRange = GetSlotRangeDisplay();
                return $"{days} {slotRange}: PKR {Price:N0}";
            }
        }

        public string DisplayName
        {
            get
            {
                if (DayNames == null || DayNames.Length == 0)
                    return GetSlotRangeDisplay();
                
                var days = string.Join(", ", DayNames.Select(d => d.Substring(0, 3)));
                return $"{days} {GetSlotRangeDisplay()}";
            }
        }

        private string GetSlotRangeDisplay()
        {
            if (SlotIds == null || SlotIds.Length == 0)
                return "No slots";
            
            var sortedSlots = SlotIds.OrderBy(s => s).ToArray();
            var firstSlot = TimeSlot.GetSlotById(sortedSlots[0]);
            var lastSlot = TimeSlot.GetSlotById(sortedSlots[sortedSlots.Length - 1]);
            
            var startDisplay = FormatHour(sortedSlots[0]);
            var endHour = (sortedSlots[sortedSlots.Length - 1] + 1) % 24;
            var endDisplay = FormatHour(endHour);
            
            return $"{startDisplay} - {endDisplay}";
        }

        private string FormatHour(int hour)
        {
            var displayHour = hour % 12;
            if (displayHour == 0) displayHour = 12;
            var period = hour >= 12 ? "PM" : "AM";
            return $"{displayHour}:00 {period}";
        }
    }
}

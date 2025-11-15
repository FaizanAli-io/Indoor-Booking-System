namespace IndoorBookingSystem.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        // Static method to get all 24 slots
        public static List<TimeSlot> GetAllSlots()
        {
            var slots = new List<TimeSlot>();
            
            for (int hour = 0; hour < 24; hour++)
            {
                var nextHour = (hour + 1) % 24;
                
                slots.Add(new TimeSlot
                {
                    Id = hour,
                    StartTime = TimeSpan.FromHours(hour),
                    EndTime = TimeSpan.FromHours(nextHour == 0 ? 24 : nextHour),
                    DisplayName = FormatSlotDisplay(hour, nextHour)
                });
            }
            
            return slots;
        }

        // Get single slot by ID
        public static TimeSlot GetSlotById(int slotId)
        {
            if (slotId < 0 || slotId > 23)
                throw new ArgumentException("Slot ID must be between 0 and 23");
            
            var nextHour = (slotId + 1) % 24;
            
            return new TimeSlot
            {
                Id = slotId,
                StartTime = TimeSpan.FromHours(slotId),
                EndTime = TimeSpan.FromHours(nextHour == 0 ? 24 : nextHour),
                DisplayName = FormatSlotDisplay(slotId, nextHour)
            };
        }

        // Format slot for display (12-hour format)
        private static string FormatSlotDisplay(int startHour, int endHour)
        {
            var startDisplay = startHour % 12;
            if (startDisplay == 0) startDisplay = 12;
            var startPeriod = startHour >= 12 ? "PM" : "AM";
            
            var endDisplay = endHour % 12;
            if (endDisplay == 0) endDisplay = 12;
            var endPeriod = endHour >= 12 ? "PM" : "AM";
            
            return $"{startDisplay}:00 {startPeriod} - {endDisplay}:00 {endPeriod}";
        }

        // Convert old string format to slot ID
        public static int ConvertStringToSlotId(string timeSlot)
        {
            // Parse "06:00-07:00" format
            var parts = timeSlot.Split('-');
            if (parts.Length != 2) return 0;
            
            var startParts = parts[0].Split(':');
            if (startParts.Length < 2) return 0;
            
            if (int.TryParse(startParts[0], out int hour))
            {
                return hour;
            }
            
            return 0;
        }
    }
}

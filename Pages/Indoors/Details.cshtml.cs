using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IndoorBookingSystem.Pages.Indoors
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DetailsModel(ApplicationDbContext context) => _context = context;

        public Indoor Indoor { get; set; } = new();
        public bool IsAdmin { get; set; }
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public List<TimeSlot> AllTimeSlots { get; set; } = TimeSlot.GetAllSlots();
        public List<int> BookedSlotIds { get; set; } = new();
        public Dictionary<int, decimal> SlotPrices { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id, DateTime? selectedDate)
        {
            var indoor = await _context.Indoors.FirstOrDefaultAsync(i => i.Id == id);
            if (indoor == null)
                return NotFound();

            Indoor = indoor;
            IsAdmin = HttpContext.Session.GetString("UserRole") == "ADMIN";
            SelectedDate = selectedDate ?? DateTime.Today;

            // Generate prices for all 24 slots (0-23)
            for (int slotId = 0; slotId < 24; slotId++)
            {
                SlotPrices[slotId] = Indoor.GetPriceForSlot(SelectedDate, slotId);
            }

            // Get booked slot IDs for the selected date
            var targetDate = SelectedDate.Date;
            var allBookings = await _context.Bookings
                .Where(b => b.IndoorId == id)
                .ToListAsync();

            BookedSlotIds = allBookings
                .Where(b => b.BookingDate.Date == targetDate &&
                           (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                .Select(b => b.SlotId)
                .ToList();

            return Page();
        }
    }
}

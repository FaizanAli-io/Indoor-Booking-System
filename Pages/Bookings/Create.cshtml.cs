using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IndoorBookingSystem.Pages.Bookings
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CreateModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public List<int> SelectedSlotIds { get; set; } = new();

        [BindProperty]
        public DateTime SelectedDate { get; set; }

        [BindProperty]
        public string IndoorId { get; set; } = string.Empty;

        public Indoor? Indoor { get; set; }
        public Dictionary<int, bool> SlotAvailability { get; set; } = new();
        public Dictionary<int, decimal> SlotPrices { get; set; } = new();
        public List<TimeSlot> AllTimeSlots { get; set; } = TimeSlot.GetAllSlots();

        public async Task<IActionResult> OnGetAsync(string? indoorId, DateTime? date)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "CLIENT" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            if (string.IsNullOrEmpty(indoorId))
                return RedirectToPage("/Indoors/Index");

            Indoor = await _context.Indoors.FirstOrDefaultAsync(i => i.Id == indoorId);
            if (Indoor == null)
                return NotFound();

            IndoorId = indoorId;
            SelectedDate = date ?? DateTime.Today;

            await LoadSlotsData();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "CLIENT" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            Indoor = await _context.Indoors.FirstOrDefaultAsync(i => i.Id == IndoorId);
            if (Indoor == null)
                return NotFound();

            if (SelectedSlotIds == null || SelectedSlotIds.Count == 0)
            {
                ModelState.AddModelError("", "Please select at least one time slot");
                await LoadSlotsData();
                return Page();
            }

            // Create multiple bookings (one per slot)
            var bookings = new List<Booking>();
            foreach (var slotId in SelectedSlotIds)
            {
                var price = Indoor.GetPriceForSlot(SelectedDate, slotId);
                
                bookings.Add(new Booking
                {
                    Id = Guid.NewGuid().ToString(),
                    PartitionKey = "Booking",
                    IndoorId = IndoorId,
                    CustomerId = userId,
                    BookingDate = SelectedDate,
                    SlotId = slotId,
                    Price = price,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Bookings.AddRange(bookings);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully booked {SelectedSlotIds.Count} slot(s)!";
            return RedirectToPage("/Bookings/Index");
        }

        private async Task LoadSlotsData()
        {
            var targetDate = SelectedDate.Date;
            var allBookings = await _context.Bookings
                .Where(b => b.IndoorId == IndoorId)
                .ToListAsync();

            var bookedSlotIds = allBookings
                .Where(b => b.BookingDate.Date == targetDate &&
                           (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                .Select(b => b.SlotId)
                .ToList();

            // Ensure pricing rules are loaded
            if (Indoor != null && Indoor.PricingRules == null)
            {
                Indoor.PricingRules = new List<PricingRule>();
            }

            // Generate all 24 slots (0-23)
            for (int slotId = 0; slotId < 24; slotId++)
            {
                SlotAvailability[slotId] = !bookedSlotIds.Contains(slotId);
                
                // Calculate price for this specific date and slot
                var price = Indoor?.PricePerHour ?? 0;
                if (Indoor != null && Indoor.PricingRules != null && Indoor.PricingRules.Count > 0)
                {
                    price = Indoor.GetPriceForSlot(SelectedDate, slotId);
                }
                SlotPrices[slotId] = price;
            }
        }
    }
}

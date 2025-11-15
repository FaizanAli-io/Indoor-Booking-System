using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IndoorBookingSystem.Pages.Indoors
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<Indoor> Indoors { get; set; } = new();
        public bool IsAdmin { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? SearchLocation { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? SearchDate { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int? SearchSlotId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            IsAdmin = role == "ADMIN";

            var query = _context.Indoors.Where(i => i.PartitionKey == "Indoor");

            if (IsAdmin)
            {
                // Admin sees only their own indoors
                query = query.Where(i => i.AdminId == userId);
            }
            else
            {
                // Customer sees all indoors with search filters
                if (!string.IsNullOrWhiteSpace(SearchName))
                    query = query.Where(i => i.Name.Contains(SearchName));
                
                if (!string.IsNullOrWhiteSpace(SearchLocation))
                    query = query.Where(i => i.Location.Contains(SearchLocation));
            }

            var allIndoors = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

            // Filter by date and time slot availability (for customers only)
            if (!IsAdmin && SearchDate.HasValue && SearchSlotId.HasValue)
            {
                var searchDate = SearchDate.Value.Date;
                var slotId = SearchSlotId.Value;

                // Fetch all bookings and filter in memory (Cosmos DB doesn't support .Date in queries)
                var allBookings = await _context.Bookings.ToListAsync();
                
                // Filter for the specific date, slot, and status in memory
                var bookedIndoorIds = allBookings
                    .Where(b => b.BookingDate.Date == searchDate && 
                               b.SlotId == slotId &&
                               (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                    .Select(b => b.IndoorId)
                    .Distinct()
                    .ToList();

                // Filter out booked indoors
                Indoors = allIndoors.Where(i => !bookedIndoorIds.Contains(i.Id)).ToList();
            }
            else
            {
                Indoors = allIndoors;
            }

            return Page();
        }
    }
}

using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IndoorBookingSystem.Pages.Bookings
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<Booking> Bookings { get; set; } = new();

        // ✅ Add UserRole property
        public string? UserRole { get; set; }

        public async Task OnGetAsync()
        {
            // ✅ Fetch role from session
            UserRole = HttpContext.Session.GetString("UserRole");

            // ✅ Filter by partition key to avoid cross-partition queries in Cosmos
            Bookings = await _context.Bookings
                                     .Where(b => b.PartitionKey == "Booking")
                                     .OrderByDescending(b => b.CreatedAt)
                                     .ToListAsync();
        }
    }
}

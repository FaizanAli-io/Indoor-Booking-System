using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
namespace IndoorBookingSystem.Pages.Bookings
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DetailsModel(ApplicationDbContext context) => _context = context;

        public Booking Booking { get; set; } = new();

        // id is string (Cosmos id). Filter with partition key for correct lookup.
        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var booking = await _context.Bookings
                                        .AsNoTracking()
                                        .Where(b => b.PartitionKey == "Booking" && b.Id == id)
                                        .FirstOrDefaultAsync();

            if (booking == null) return NotFound();

            Booking = booking;
            return Page();
        }
    }
}

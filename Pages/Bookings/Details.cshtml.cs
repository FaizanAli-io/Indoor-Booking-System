using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndoorBookingSystem.Pages.Bookings
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DetailsModel(ApplicationDbContext context) => _context = context;

        public Booking Booking { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            Booking = booking;
            return Page();
        }
    }
}

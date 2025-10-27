using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndoorBookingSystem.Pages.Bookings
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Booking Booking { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
                return Forbid();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            Booking = booking;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
                return Forbid();

            if (!ModelState.IsValid) return Page();

            var existingBooking = await _context.Bookings.FindAsync(Booking.Id);
            if (existingBooking == null) return NotFound();

            // Update fields
            existingBooking.Title = Booking.Title;
            existingBooking.Description = Booking.Description;
            existingBooking.MediaUrls = Booking.MediaUrls;
            existingBooking.Price = Booking.Price;
            existingBooking.DurationHours = Booking.DurationHours;
            existingBooking.Location = Booking.Location;

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}

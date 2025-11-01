using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IndoorBookingSystem.Pages.Bookings
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Booking Booking { get; set; } = new();

        // id is Cosmos string id
        public async Task<IActionResult> OnGetAsync(string id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
                return RedirectToPage("/Auth/Login");

            if (string.IsNullOrEmpty(id)) return NotFound();

            // Load by (PartitionKey, Id)
            var booking = await _context.Bookings
                                        .AsNoTracking()
                                        .Where(b => b.PartitionKey == "Booking" && b.Id == id)
                                        .FirstOrDefaultAsync();

            if (booking == null) return NotFound();

            Booking = booking;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
                return RedirectToPage("/Auth/Login");

            // Load existing entity first (tracked) using (PartitionKey, Id)
            var existing = await _context.Bookings
                                         .Where(b => b.PartitionKey == "Booking" && b.Id == Booking.Id)
                                         .FirstOrDefaultAsync();
            if (existing == null) return NotFound();

            // Update allowed fields
            existing.Title = Booking.Title;
            existing.Description = Booking.Description;
            existing.MediaUrls = Booking.MediaUrls;
            existing.Price = Booking.Price; // updates PriceCents via setter
            existing.DurationHours = Booking.DurationHours;
            existing.Location = Booking.Location;

            // Optional: normalize MediaUrls if a single comma-separated string was posted
            if (existing.MediaUrls is { Length: 1 })
            {
                var single = existing.MediaUrls[0];
                if (!string.IsNullOrWhiteSpace(single) && single.Contains(','))
                {
                    existing.MediaUrls = single
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }
            }

            // Validate the entity we are saving (has required PartitionKey/UserId already)
            ModelState.Clear();
            if (!TryValidateModel(existing, nameof(Booking)))
            {
                // Re-populate bound model for the page
                Booking = existing;
                return Page();
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}

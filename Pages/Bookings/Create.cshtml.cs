using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndoorBookingSystem.Pages.Bookings
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CreateModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Booking Booking { get; set; } = new();

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN")
                return RedirectToPage("/Auth/Login"); // redirect instead of Forbid

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN")
                return RedirectToPage("/Auth/Login"); // redirect instead of Forbid

            // Ensure required fields for Cosmos single-container strategy
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login"); // or ModelState.AddModelError("", "Not signed in")

            Booking.UserId = userId;
            Booking.PartitionKey ??= "Booking";
            Booking.CreatedAt = DateTime.UtcNow;
            Booking.Id ??= Guid.NewGuid().ToString();

            // Normalize MediaUrls if the form sends a single comma-separated string
            if (Booking.MediaUrls is { Length: 1 })
            {
                var single = Booking.MediaUrls[0];
                if (!string.IsNullOrWhiteSpace(single) && single.Contains(','))
                {
                    Booking.MediaUrls = single
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }
            }

            // Re-validate after populating required properties
            ModelState.ClearValidationState(nameof(Booking));
            if (!TryValidateModel(Booking, nameof(Booking)))
                return Page();

            _context.Bookings.Add(Booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}

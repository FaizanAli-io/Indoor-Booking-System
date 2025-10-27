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

            if (!ModelState.IsValid) return Page();

            Booking.CreatedAt = DateTime.Now;
            _context.Bookings.Add(Booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}

using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IndoorBookingSystem.Pages.Bookings
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<Booking> Bookings { get; set; } = new();
        public Dictionary<string, Indoor> IndoorDetails { get; set; } = new();
        public Dictionary<string, User> CustomerDetails { get; set; } = new();
        public string? UserRole { get; set; }
        public string? UserId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            UserRole = HttpContext.Session.GetString("UserRole");
            UserId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(UserRole) || string.IsNullOrEmpty(UserId))
                return RedirectToPage("/Auth/Login");

            if (UserRole == "ADMIN")
            {
                // Admin sees bookings for their indoors
                var adminIndoorIds = await _context.Indoors
                    .Where(i => i.AdminId == UserId)
                    .Select(i => i.Id)
                    .ToListAsync();

                Bookings = await _context.Bookings
                    .Where(b => adminIndoorIds.Contains(b.IndoorId))
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                // Load customer details
                var customerIds = Bookings.Select(b => b.CustomerId).Distinct().ToList();
                var customers = await _context.Users.Where(u => customerIds.Contains(u.Id)).ToListAsync();
                CustomerDetails = customers.ToDictionary(c => c.Id);
            }
            else
            {
                // Customer sees their own bookings
                Bookings = await _context.Bookings
                    .Where(b => b.CustomerId == UserId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
            }

            // Load indoor details
            var indoorIds = Bookings.Select(b => b.IndoorId).Distinct().ToList();
            var indoors = await _context.Indoors.Where(i => indoorIds.Contains(i.Id)).ToListAsync();
            IndoorDetails = indoors.ToDictionary(i => i.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(string bookingId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "ADMIN" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                return NotFound();

            booking.Status = BookingStatus.Confirmed;
            booking.ConfirmedByAdminId = userId;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(string bookingId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "ADMIN" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                return NotFound();

            booking.Status = BookingStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}

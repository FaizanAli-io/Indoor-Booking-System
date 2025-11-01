using IndoorBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IndoorBookingSystem.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;

        public LoginModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _authService.ValidateUser(Email, Password);
            if (user == null)
            {
                ErrorMessage = "Invalid credentials";
                return Page();
            }

            // ✅ Save user info in session
            HttpContext.Session.SetString("UserRole", user.Role.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserId", user.Id);

            // Redirect to bookings page or dashboard
            return RedirectToPage("/Bookings/Index");
        }

    }
}

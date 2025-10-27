using IndoorBookingSystem.Models;
using IndoorBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndoorBookingSystem.Pages.Auth
{
    public class SignupModel : PageModel
    {
        private readonly AuthService _authService;

        public SignupModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public User User { get; set; } = new User();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var success = await _authService.RegisterUser(User);
            if (!success)
            {
                ErrorMessage = "Email already registered";
                return Page();
            }

            return RedirectToPage("/Auth/Login");
        }
    }
}

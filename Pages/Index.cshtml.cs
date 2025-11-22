using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndoorBookingSystem.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole))
            {
                // Azure AD authenticated but not logged into app - show welcome page
                return Page();
            }

            // Redirect based on role
            return RedirectToPage("/Indoors/Index");
        }
    }
}

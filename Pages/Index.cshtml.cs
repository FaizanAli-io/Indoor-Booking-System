using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndoorBookingSystem.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole))
            {
                // Not logged in - show welcome page
                return Page();
            }

            // Redirect based on role
            return RedirectToPage("/Indoors/Index");
        }
    }
}

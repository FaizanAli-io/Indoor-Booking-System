using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using IndoorBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndoorBookingSystem.Pages.Indoors
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobService;

        public CreateModel(ApplicationDbContext context, BlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        [BindProperty]
        public Indoor Indoor { get; set; } = new();

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN")
                return RedirectToPage("/Auth/Login");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFileCollection mediaFiles)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "ADMIN")
                return RedirectToPage("/Auth/Login");

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            Indoor.AdminId = userId;
            Indoor.PartitionKey = "Indoor";
            Indoor.CreatedAt = DateTime.UtcNow;
            Indoor.Id = Guid.NewGuid().ToString();

            // Upload media files to Blob Storage
            if (mediaFiles != null && mediaFiles.Count > 0)
            {
                Indoor.MediaUrls = (await _blobService.UploadMultipleFilesAsync(mediaFiles)).ToArray();
            }

            ModelState.ClearValidationState(nameof(Indoor));
            if (!TryValidateModel(Indoor, nameof(Indoor)))
                return Page();

            _context.Indoors.Add(Indoor);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}

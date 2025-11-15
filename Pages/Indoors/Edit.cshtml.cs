using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using IndoorBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IndoorBookingSystem.Pages.Indoors
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobService;

        public EditModel(ApplicationDbContext context, BlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        [BindProperty]
        public Indoor Indoor { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "ADMIN" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var indoor = await _context.Indoors.FirstOrDefaultAsync(i => i.Id == id && i.AdminId == userId);
            if (indoor == null)
                return NotFound();

            Indoor = indoor;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFileCollection mediaFiles)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "ADMIN" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            // Verify ownership
            var existing = await _context.Indoors.FirstOrDefaultAsync(i => i.Id == Indoor.Id && i.AdminId == userId);
            if (existing == null)
                return NotFound();

            ModelState.ClearValidationState(nameof(Indoor));
            if (!TryValidateModel(Indoor, nameof(Indoor)))
                return Page();

            // Update basic info only
            existing.Name = Indoor.Name;
            existing.Location = Indoor.Location;
            existing.Description = Indoor.Description;
            existing.PricePerHourCents = Indoor.PricePerHourCents;

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = Indoor.Id });
        }

        public async Task<IActionResult> OnPostUploadMediaAsync(IFormFileCollection mediaFiles)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "ADMIN" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var existing = await _context.Indoors.FirstOrDefaultAsync(i => i.Id == Indoor.Id && i.AdminId == userId);
            if (existing == null)
                return NotFound();

            // Upload new media files
            if (mediaFiles != null && mediaFiles.Count > 0)
            {
                var newUrls = await _blobService.UploadMultipleFilesAsync(mediaFiles);
                existing.MediaUrls = existing.MediaUrls.Concat(newUrls).ToArray();
                
                // Mark entity as modified for Cosmos DB
                _context.Entry(existing).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"✅ Successfully uploaded {mediaFiles.Count} file(s)!";
            }

            return RedirectToPage(new { id = Indoor.Id });
        }

        public async Task<IActionResult> OnPostDeleteMediaAsync(string id, int mediaIndex)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "ADMIN" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var existing = await _context.Indoors.FirstOrDefaultAsync(i => i.Id == id && i.AdminId == userId);
            if (existing == null)
                return NotFound();

            // Validate index
            if (mediaIndex < 0 || mediaIndex >= existing.MediaUrls.Length)
            {
                TempData["ErrorMessage"] = "❌ Invalid media file index.";
                return RedirectToPage(new { id });
            }

            // Get the URL to delete
            var urlToDelete = existing.MediaUrls[mediaIndex];

            // Delete from blob storage
            try
            {
                await _blobService.DeleteFileAsync(urlToDelete);
            }
            catch (Exception ex)
            {
                // Log error but continue - file might already be deleted from storage
                Console.WriteLine($"Error deleting blob: {ex.Message}");
            }

            // Remove from array
            var updatedUrls = existing.MediaUrls.Where((url, index) => index != mediaIndex).ToArray();
            existing.MediaUrls = updatedUrls;

            // Mark entity as modified for Cosmos DB
            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Media file deleted successfully!";
            return RedirectToPage(new { id });
        }

        // Pricing management has been moved to dedicated Pricing page
        // Use the "Manage Pricing" button to access pricing rules
    }
}

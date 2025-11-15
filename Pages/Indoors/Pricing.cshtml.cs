using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IndoorBookingSystem.Pages.Indoors
{
    public class PricingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PricingModel(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public async Task<IActionResult> OnPostAddRuleAsync(string id, List<DayOfWeek> selectedDays, int[] selectedSlots, decimal price)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "ADMIN" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var indoor = await _context.Indoors.FirstOrDefaultAsync(i => i.Id == id && i.AdminId == userId);
            if (indoor == null)
                return NotFound();

            if (selectedDays == null || selectedDays.Count == 0)
            {
                ModelState.AddModelError("", "❌ Please select at least one day");
                Indoor = indoor;
                return Page();
            }

            if (selectedSlots == null || selectedSlots.Length == 0)
            {
                ModelState.AddModelError("", "❌ Please select at least one time slot");
                Indoor = indoor;
                return Page();
            }

            indoor.PricingRules ??= new List<PricingRule>();

            // Check for conflicts with existing rules
            var conflicts = new List<string>();
            for (int i = 0; i < indoor.PricingRules.Count; i++)
            {
                var existingRule = indoor.PricingRules[i];
                
                // Skip old format rules (they will be ignored)
                if (existingRule.SlotIds == null || existingRule.SlotIds.Length == 0)
                {
                    continue;
                }
                
                // Check if there's any day overlap
                var dayOverlap = selectedDays.Any(day => existingRule.ApplicableDays.Contains(day));
                
                if (dayOverlap)
                {
                    // Check if there's any slot overlap
                    var slotOverlap = selectedSlots.Any(slot => existingRule.SlotIds.Contains(slot));
                    
                    if (slotOverlap)
                    {
                        var overlappingDays = selectedDays.Where(day => existingRule.ApplicableDays.Contains(day));
                        var daysStr = string.Join(", ", overlappingDays.Select(d => d.ToString().Substring(0, 3)));
                        var ruleNum = i + 1;
                        
                        conflicts.Add($"Rule #{ruleNum}: {daysStr} (Slots: {string.Join(",", existingRule.SlotIds)}) - PKR {existingRule.Price:N0}");
                    }
                }
            }

            if (conflicts.Count > 0)
            {
                var errorMsg = "⚠️ <strong>Pricing Conflict Detected!</strong><br/><br/>" +
                              "The new rule overlaps with existing rule(s):<br/><ul>";
                
                foreach (var conflict in conflicts)
                {
                    errorMsg += $"<li>{conflict}</li>";
                }
                
                errorMsg += "</ul><br/><strong>Solution:</strong> Please delete the conflicting rule(s) first, then create your new rule.";
                
                ModelState.AddModelError("", errorMsg);
                Indoor = indoor;
                return Page();
            }

            // No conflicts, add the new rule
            indoor.PricingRules.Add(new PricingRule
            {
                ApplicableDays = selectedDays,
                SlotIds = selectedSlots.OrderBy(s => s).ToArray(),
                Price = price
            });

            // Mark entire entity as modified for Cosmos DB
            _context.Entry(indoor).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Pricing rule added successfully!";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteRuleAsync(string id, int ruleIndex)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "ADMIN" || string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var indoor = await _context.Indoors.FirstOrDefaultAsync(i => i.Id == id && i.AdminId == userId);
            if (indoor == null)
                return NotFound();

            if (indoor.PricingRules != null && ruleIndex >= 0 && ruleIndex < indoor.PricingRules.Count)
            {
                indoor.PricingRules.RemoveAt(ruleIndex);
                
                // Mark entire entity as modified for Cosmos DB
                _context.Entry(indoor).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "✅ Pricing rule deleted successfully!";
            }

            return RedirectToPage(new { id });
        }
    }
}

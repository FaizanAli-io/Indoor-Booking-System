using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndoorBookingSystem.Pages
{
    public class InsightsModel : PageModel
    {
        private readonly TelemetryClient _telemetry;
        private readonly ILogger<InsightsModel> _logger;

        public InsightsModel(TelemetryClient telemetry, ILogger<InsightsModel> logger)
        {
            _telemetry = telemetry;
            _logger = logger;
        }

        [BindProperty]
        public string EventName { get; set; } = "CustomEvent";

        [BindProperty]
        public string Message { get; set; } = string.Empty;

        public string StatusMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            // optional default values
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(EventName))
            {
                StatusMessage = "Event name is required.";
                return Page();
            }

            var properties = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Message))
            {
                properties["message"] = Message;
            }

            // Track a custom event and a trace
            _telemetry.TrackEvent(EventName, properties);
            if (!string.IsNullOrWhiteSpace(Message))
            {
                _telemetry.TrackTrace(Message);
            }

            _logger.LogInformation("Tracked Application Insights event {EventName}", EventName);

            StatusMessage = $"Tracked event '{EventName}'";
            return Page();
        }
    }
}
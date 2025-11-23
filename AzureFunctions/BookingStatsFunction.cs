using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;

namespace IndoorBookingFunctions
{
    public class BookingStatsFunction
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;

        public BookingStatsFunction(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
        {
            _logger = loggerFactory.CreateLogger<BookingStatsFunction>();
            _cosmosClient = cosmosClient;
        }

        [Function("BookingStats")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("BookingStats function triggered");

            try
            {
                var container = _cosmosClient.GetContainer("IndoorBookingDB", "Bookings");
                
                // Count total bookings
                var totalQuery = "SELECT VALUE COUNT(1) FROM c";
                var totalIterator = container.GetItemQueryIterator<int>(totalQuery);
                var totalBookings = 0;
                while (totalIterator.HasMoreResults)
                {
                    var response = await totalIterator.ReadNextAsync();
                    totalBookings = response.FirstOrDefault();
                }

                // Count confirmed bookings
                var confirmedQuery = "SELECT VALUE COUNT(1) FROM c WHERE c.Status = 'Confirmed'";
                var confirmedIterator = container.GetItemQueryIterator<int>(confirmedQuery);
                var confirmedBookings = 0;
                while (confirmedIterator.HasMoreResults)
                {
                    var response = await confirmedIterator.ReadNextAsync();
                    confirmedBookings = response.FirstOrDefault();
                }

                // Count pending bookings
                var pendingQuery = "SELECT VALUE COUNT(1) FROM c WHERE c.Status = 'Pending'";
                var pendingIterator = container.GetItemQueryIterator<int>(pendingQuery);
                var pendingBookings = 0;
                while (pendingIterator.HasMoreResults)
                {
                    var response = await pendingIterator.ReadNextAsync();
                    pendingBookings = response.FirstOrDefault();
                }

                var stats = new
                {
                    Message = "Indoor Booking System Statistics",
                    TotalBookings = totalBookings,
                    ConfirmedBookings = confirmedBookings,
                    PendingBookings = pendingBookings,
                    RejectedBookings = totalBookings - confirmedBookings - pendingBookings,
                    Timestamp = DateTime.UtcNow
                };

                var httpResponse = req.CreateResponse(HttpStatusCode.OK);
                httpResponse.Headers.Add("Content-Type", "application/json");
                await httpResponse.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(stats));
                
                return httpResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}

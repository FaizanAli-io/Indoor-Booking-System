using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;

namespace My.Function
{
    // =========================
    // 1) USER MODELS
    // =========================
    public class UserItem
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        public string CreatedAt { get; set; }
        public string Email { get; set; }
        public string EntityType { get; set; }
        public string FullName { get; set; }
        public string PartitionKey { get; set; }
        public int Role { get; set; }

        // present in DB but not needed in response
        public string Password { get; set; }
    }

    public class UserResponse
    {
        public string FullName { get; set; }
        public string EntityType { get; set; }
        public string Email { get; set; }
        public string CreatedAt { get; set; }
        public int Role { get; set; }
    }

    // =========================
    // 2) INDOOR MODELS
    // =========================
    public class IndoorItem
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        public string AdminId { get; set; }
        public string CreatedAt { get; set; }
        public string Description { get; set; }
        public string EntityType { get; set; }
        public string Location { get; set; }

        public object MediaUrls { get; set; }

        public string Name { get; set; }
        public string PartitionKey { get; set; }
        public int PricePerHourCents { get; set; }
    }

    public class IndoorResponse
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public object MediaUrls { get; set; }
        public int PricePerHourCents { get; set; }
        public string CreatedAt { get; set; }
        public string AdminId { get; set; }
    }

    // =========================
    // 3) BOOKING MODELS
    // =========================
    public class BookingItem
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        public string BookingDate { get; set; }
        public string CreatedAt { get; set; }
        public string CustomerId { get; set; }
        public string IndoorId { get; set; }
        public string EntityType { get; set; }
        public string PartitionKey { get; set; }

        public int PriceCents { get; set; }
        public int SlotId { get; set; }
        public int Status { get; set; }
    }

    public class BookingResponse
    {
        public string id { get; set; }
        public string BookingDate { get; set; }
        public string CreatedAt { get; set; }
        public string CustomerId { get; set; }
        public string IndoorId { get; set; }
        public int PriceCents { get; set; }
        public int SlotId { get; set; }
        public int Status { get; set; }
        public string EntityType { get; set; }
    }

    // =========================
    // FUNCTION
    // =========================
    public class IndoorBooking
    {
        private readonly ILogger<IndoorBooking> _logger;
        private readonly Container _container;

        private const string DatabaseName = "IndoorBookingDB";
        private const string ContainerName = "AppData";

        public IndoorBooking(ILogger<IndoorBooking> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _container = cosmosClient.GetContainer(DatabaseName, ContainerName);
        }

        [Function("IndoorBooking")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "IndoorBooking")]
            HttpRequestData req,
            FunctionContext context)
        {
            _logger.LogInformation("IndoorBooking: Querying Cosmos DB for Users, Indoors, and Bookings...");

            try
            {
                // -------------------------
                // USERS QUERY
                // -------------------------
                var usersQuery = new QueryDefinition(
                        "SELECT * FROM c WHERE c.PartitionKey = @pk")
                    .WithParameter("@pk", "User");

                var usersIterator = _container.GetItemQueryIterator<UserItem>(
                    usersQuery,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey("User")
                    });

                var usersRaw = new List<UserItem>();
                while (usersIterator.HasMoreResults)
                {
                    var response = await usersIterator.ReadNextAsync();
                    usersRaw.AddRange(response);
                }

                var users = usersRaw.Select(u => new UserResponse
                {
                    FullName = u.FullName,
                    EntityType = u.EntityType,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt,
                    Role = u.Role
                }).ToList();


                // -------------------------
                // INDOORS QUERY
                // -------------------------
                var indoorsQuery = new QueryDefinition(
                        "SELECT * FROM c WHERE c.PartitionKey = @pk")
                    .WithParameter("@pk", "Indoor");

                var indoorsIterator = _container.GetItemQueryIterator<IndoorItem>(
                    indoorsQuery,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey("Indoor")
                    });

                var indoorsRaw = new List<IndoorItem>();
                while (indoorsIterator.HasMoreResults)
                {
                    var response = await indoorsIterator.ReadNextAsync();
                    indoorsRaw.AddRange(response);
                }

                var indoors = indoorsRaw.Select(i => new IndoorResponse
                {
                    id = i.id,
                    Name = i.Name,
                    Description = i.Description,
                    Location = i.Location,
                    MediaUrls = i.MediaUrls,
                    PricePerHourCents = i.PricePerHourCents,
                    CreatedAt = i.CreatedAt,
                    AdminId = i.AdminId
                }).ToList();


                // -------------------------
                // BOOKINGS QUERY
                // -------------------------
                var bookingsQuery = new QueryDefinition(
                        "SELECT * FROM c WHERE c.PartitionKey = @pk")
                    .WithParameter("@pk", "Booking");

                var bookingsIterator = _container.GetItemQueryIterator<BookingItem>(
                    bookingsQuery,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey("Booking")
                    });

                var bookingsRaw = new List<BookingItem>();
                while (bookingsIterator.HasMoreResults)
                {
                    var response = await bookingsIterator.ReadNextAsync();
                    bookingsRaw.AddRange(response);
                }

                var bookings = bookingsRaw.Select(b => new BookingResponse
                {
                    id = b.id,
                    BookingDate = b.BookingDate,
                    CreatedAt = b.CreatedAt,
                    CustomerId = b.CustomerId,
                    IndoorId = b.IndoorId,
                    PriceCents = b.PriceCents,
                    SlotId = b.SlotId,
                    Status = b.Status,
                    EntityType = b.EntityType
                }).ToList();


                // -------------------------
                // BUILD HTML PAGE
                // -------------------------
                var html = BuildHtmlPage(users, indoors, bookings);

                var responseMessage = req.CreateResponse(HttpStatusCode.OK);
                responseMessage.Headers.Add("Content-Type", "text/html; charset=utf-8");
                await responseMessage.WriteStringAsync(html);

                return responseMessage;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError($"Cosmos DB error: {cosmosEx.StatusCode} – {cosmosEx.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error querying Cosmos DB.");
                return errorResponse;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("General error.");
                return errorResponse;
            }
        }

        // =========================
        // HTML BUILDER
        // =========================
        private static string BuildHtmlPage(
            List<UserResponse> users,
            List<IndoorResponse> indoors,
            List<BookingResponse> bookings)
        {
            string usersRows = string.Join("", users.Select(u => $@"
                <tr>
                    <td>{u.FullName}</td>
                    <td>{u.EntityType}</td>
                    <td>{u.Email}</td>
                    <td>{u.CreatedAt}</td>
                    <td>{u.Role}</td>
                </tr>
            "));

            string indoorsRows = string.Join("", indoors.Select(i => $@"
                <tr>
                    <td>{i.id}</td>
                    <td>{i.Name}</td>
                    <td>{i.Location}</td>
                    <td>{i.PricePerHourCents}</td>
                    <td>{i.CreatedAt}</td>
                </tr>
            "));

            string bookingsRows = string.Join("", bookings.Select(b => $@"
                <tr>
                    <td>{b.id}</td>
                    <td>{b.BookingDate}</td>
                    <td>{b.CustomerId}</td>
                    <td>{b.IndoorId}</td>
                    <td>{b.PriceCents}</td>
                    <td>{b.SlotId}</td>
                    <td>{b.Status}</td>
                </tr>
            "));

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'/>
    <meta name='viewport' content='width=device-width, initial-scale=1'/>
    <title>IndoorBooking Dashboard</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background: #0b1220;
            color: #e5e7eb;
            margin: 0;
            padding: 20px;
        }}
        h1 {{
            margin: 0 0 10px 0;
            font-size: 26px;
        }}
        .section {{
            background: #111827;
            padding: 16px;
            border-radius: 12px;
            margin-bottom: 18px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.4);
        }}
        .section h2 {{
            margin-top: 0;
            font-size: 20px;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
            font-size: 14px;
        }}
        th, td {{
            text-align: left;
            padding: 8px;
            border-bottom: 1px solid #1f2937;
        }}
        th {{
            background: #0f172a;
            position: sticky;
            top: 0;
        }}
        tr:hover {{
            background: #0f172a;
        }}
        .count {{
            font-size: 13px;
            color: #9ca3af;
        }}
    </style>
</head>
<body>

    <h1>IndoorBookingDB / AppData Dashboard</h1>
    <p class='count'>Users: {users.Count} | Indoors: {indoors.Count} | Bookings: {bookings.Count}</p>

    <div class='section'>
        <h2>Users</h2>
        <table>
            <thead>
                <tr>
                    <th>Full Name</th>
                    <th>Entity Type</th>
                    <th>Email</th>
                    <th>Created At</th>
                    <th>Role</th>
                </tr>
            </thead>
            <tbody>
                {usersRows}
            </tbody>
        </table>
    </div>

    <div class='section'>
        <h2>Indoors</h2>
        <table>
            <thead>
                <tr>
                    <th>Indoor ID</th>
                    <th>Name</th>
                    <th>Location</th>
                    <th>Price/Hour (cents)</th>
                    <th>Created At</th>
                </tr>
            </thead>
            <tbody>
                {indoorsRows}
            </tbody>
        </table>
    </div>

    <div class='section'>
        <h2>Bookings</h2>
        <table>
            <thead>
                <tr>
                    <th>Booking ID</th>
                    <th>Date</th>
                    <th>Customer ID</th>
                    <th>Indoor ID</th>
                    <th>Price (cents)</th>
                    <th>Slot</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
                {bookingsRows}
            </tbody>
        </table>
    </div>

</body>
</html>";
        }
    }
}

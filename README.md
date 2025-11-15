# Indoor Booking System

A complete ASP.NET Core Razor Pages application for managing indoor sports facility bookings with separate portals for admins and customers.

## Features

### Admin Portal
- **Indoor Management**: Add, edit, and manage indoor facilities
- **Media Upload**: Upload images and videos to Azure Blob Storage
- **Dynamic Pricing**: Set different prices based on day of week and time slots
- **Booking Management**: View and confirm/reject customer bookings
- **Multi-venue Support**: One admin can manage multiple indoor facilities

### Customer Portal
- **Browse Indoors**: Search facilities by name and location
- **View Availability**: Check real-time slot availability (1-hour slots, 9 AM - 9 PM)
- **Dynamic Pricing Display**: See prices that vary by day and time
- **Book Facilities**: Reserve time slots for indoor facilities
- **Booking Status**: Track booking status (Pending/Confirmed/Rejected)

## Technology Stack

- **Framework**: ASP.NET Core 8.0 (Razor Pages)
- **Database**: Azure Cosmos DB (NoSQL)
- **Storage**: Azure Blob Storage (for images/videos)
- **Authentication**: Session-based with role management (Admin/Client)

## Database Structure

All data is stored in Cosmos DB using a single container strategy:

### Collections:
1. **Users** - Admin and customer accounts
2. **Indoors** - Indoor facility listings
3. **Bookings** - Customer booking records

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK or higher
- Azure Cosmos DB account
- Azure Blob Storage account

### Configuration

1. **Clone the repository**

2. **Update `appsettings.json`** with your Azure credentials:
```json
{
  "Cosmos": {
    "AccountEndpoint": "YOUR_COSMOS_ENDPOINT",
    "AccountKey": "YOUR_COSMOS_KEY",
    "DatabaseName": "IndoorBookingDB"
  },
  "BlobStorage": {
    "ConnectionString": "YOUR_BLOB_CONNECTION_STRING",
    "ContainerName": "indoor-media"
  }
}
```

3. **Restore packages**:
```bash
dotnet restore
```

4. **Build the project**:
```bash
dotnet build
```

5. **Run the application**:
```bash
dotnet run
```

6. **Access the app**:
   - Open browser to `https://localhost:5001` or `http://localhost:5000`

## Usage

### First Time Setup

1. **Sign Up as Admin**:
   - Go to Sign Up page
   - Select "ADMIN" role
   - Create account

2. **Add Indoor Facilities**:
   - Login as admin
   - Navigate to "My Indoors"
   - Click "Add New Indoor"
   - Fill in details and upload media
   - Submit

3. **Sign Up as Customer**:
   - Create another account with "CLIENT" role

4. **Make a Booking**:
   - Login as customer
   - Browse indoors
   - Select a facility
   - Choose date and time slot
   - Confirm booking

5. **Manage Bookings** (Admin):
   - Login as admin
   - Go to "Manage Bookings"
   - Confirm or reject pending bookings

## Project Structure

```
IndoorBookingSystem/
├── Data/
│   └── ApplicationDbContext.cs      # EF Core DbContext
├── Models/
│   ├── User.cs                      # User model (Admin/Client)
│   ├── Indoor.cs                    # Indoor facility model
│   └── Booking.cs                   # Booking model
├── Services/
│   ├── AuthService.cs               # Authentication logic
│   └── BlobStorageService.cs        # Azure Blob Storage operations
├── Pages/
│   ├── Auth/                        # Login/Signup pages
│   ├── Indoors/                     # Indoor management pages
│   │   ├── Index.cshtml             # Browse/List indoors
│   │   ├── Create.cshtml            # Add new indoor
│   │   ├── Edit.cshtml              # Edit indoor
│   │   └── Details.cshtml           # View indoor details
│   └── Bookings/                    # Booking management pages
│       ├── Index.cshtml             # View bookings
│       └── Create.cshtml            # Create booking
└── Program.cs                       # App configuration
```

## Key Features Explained

### Dynamic Pricing System
- **Base Price**: Default price per hour for the indoor facility
- **Pricing Rules**: Set custom prices for specific days and time ranges
- **Examples**:
  - Friday & Saturday, 6:00 PM - 11:00 PM: $3000 (peak hours)
  - Monday-Thursday, 6:00 AM - 5:00 PM: $2000 (off-peak)
  - Sunday, 5:00 PM - 9:00 PM: $2500 (evening rate)
- **Automatic Calculation**: Price is calculated and locked when booking is created
- **Customer View**: Customers see the applicable price for each time slot

### Media Storage
- Images and videos are uploaded to Azure Blob Storage
- SAS URLs are stored in Cosmos DB (MediaUrls array)
- Private container with secure access tokens
- Supports multiple files per indoor facility

### Time Slot System
- 1-hour slots from 9:00 AM to 9:00 PM
- Real-time availability checking
- Prevents double-booking
- Price varies by slot based on pricing rules

### Role-Based Access
- **Admin**: Can create indoors, manage bookings for their facilities
- **Client**: Can browse indoors, make bookings, view their booking history

### Booking Workflow
1. Customer selects indoor and time slot
2. Booking created with "Pending" status
3. Admin receives notification (in booking list)
4. Admin confirms or rejects
5. Customer sees updated status

## API Endpoints (Pages)

- `/` - Home page
- `/Auth/Login` - Login page
- `/Auth/Signup` - Registration page
- `/Indoors/Index` - Browse indoors (Customer) / My indoors (Admin)
- `/Indoors/Create` - Add new indoor (Admin only)
- `/Indoors/Edit/{id}` - Edit indoor (Admin only)
- `/Indoors/Pricing/{id}` - Manage pricing rules (Admin only)
- `/Indoors/Details/{id}` - View indoor details with availability
- `/Bookings/Index` - View bookings
- `/Bookings/Create?indoorId={id}` - Create booking (Customer only)

## Security Notes

- Passwords are hashed using SHA256
- Session-based authentication
- Role-based authorization on all protected pages
- Admin can only edit their own indoors
- Customers can only view their own bookings

## Future Enhancements

- Email notifications for booking confirmations
- Payment integration
- Reviews and ratings
- Advanced search filters
- Booking history and analytics
- Calendar view for availability

## Troubleshooting

### Build Errors
If you get file lock errors, stop any running instances:
```bash
# Find and kill the process
taskkill /F /IM IndoorBookingSystem.exe
```

### Database Connection Issues
- Verify Cosmos DB credentials in appsettings.json
- Check firewall settings in Azure portal
- Ensure database is created (app creates it automatically on first run)

### Blob Storage Issues
- Verify connection string is correct
- Container "indoor-media" is created automatically
- Ensure public access is set to "Blob" level

## License

This project is for educational purposes.


4:00 pm to 6:00 am means 4-5,pm 5-6pm, 6-7pm, 7-8pm, 8-9pm, 9-10pm, 10-11pm, 11pm-12am, 12-1am, 1-2am, 2-3am, 3-4am, 4-5am, 5-6am
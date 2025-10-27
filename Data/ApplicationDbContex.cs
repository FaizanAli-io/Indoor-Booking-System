using IndoorBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace IndoorBookingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure Price has proper precision for SQL Server
            modelBuilder.Entity<Booking>()
                .Property(b => b.Price)
                .HasPrecision(18, 2);
        }
    }
}

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

            modelBuilder.HasDefaultContainer("AppData");

            modelBuilder.Entity<User>()
                .HasPartitionKey(u => u.PartitionKey)
                .ToContainer("AppData")
                .HasDiscriminator<string>("EntityType")
                .HasValue("User");

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ToJsonProperty("id");

            modelBuilder.Entity<Booking>()
                .HasPartitionKey(b => b.PartitionKey)
                .ToContainer("AppData")
                .HasDiscriminator<string>("EntityType")
                .HasValue("Booking");

            modelBuilder.Entity<Booking>()
                .Property(b => b.Id)
                .ToJsonProperty("id");
        }
    }
}

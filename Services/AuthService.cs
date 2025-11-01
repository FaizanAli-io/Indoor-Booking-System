using IndoorBookingSystem.Data;
using IndoorBookingSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace IndoorBookingSystem.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterUser(User user)
        {
            // Ensure partition key is set for the single-container strategy
            user.PartitionKey ??= "User";

            // Use a top-level query instead of Any/EXISTS to avoid 'root' in subquery
            var existingId = await _context.Users
                .AsNoTracking()
                .Where(u => u.PartitionKey == "User" && u.Email == user.Email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(existingId))
                return false;

            user.Password = HashPassword(user.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> ValidateUser(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;
            return VerifyPassword(password, user.Password) ? user : null;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}

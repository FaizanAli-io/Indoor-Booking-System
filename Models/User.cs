using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace IndoorBookingSystem.Models
{
    public enum Role
    {
        ADMIN,
        CLIENT
    }

    public class User
    {
        [Key]
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PartitionKey { get; set; } = "User";

        [Required]
        [MinLength(5, ErrorMessage = "Full Name must be at least 5 characters long.")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public Role Role { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

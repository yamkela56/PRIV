using System.ComponentModel.DataAnnotations;

namespace PRIV.DTOs
{

    public class RegisterRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50), RegularExpression("^[a-zA-Z0-9_.]{3,50}$",
            ErrorMessage = "Username can only contain letters, numbers, underscores and dots.")]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

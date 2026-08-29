using System.ComponentModel.DataAnnotations;

namespace PRIV.DTOs
{
    public class LoginRequest
    {
        // Accepts either a username or an email address.
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

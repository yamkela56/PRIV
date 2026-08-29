using System.ComponentModel.DataAnnotations;

namespace PRIV.DTOs
{

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;

namespace PRIV.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public bool IsDiscoverable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
using System;

namespace PRIV.Models
{
    public class Connection
    {
        public int ConnectionId { get; set; }
        public int RequesterId { get; set; }
        public int TargetId { get; set; }
        public string Status { get; set; } = "Pending"; // 'Pending', 'Approved', 'Declined'
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? Requester { get; set; }
        public User? Target { get; set; }
    }
}
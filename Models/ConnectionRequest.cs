namespace PRIV.Models
{
    // Represents "User A wants to be able to see User B's available slots and book with them."
    // Until Status == Approved, User A sees no slots and cannot submit booking requests.
    public class ConnectionRequest
    {
        public int Id { get; set; }

        public int RequesterId { get; set; } // User A
        public User Requester { get; set; } = null!;

        public int TargetId { get; set; } // User B
        public User Target { get; set; } = null!;

        public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}

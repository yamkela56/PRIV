namespace PRIV.DTOs
{
    public class ConnectionResponseDto
    {
        public int ConnectionId { get; set; }
        public string Action { get; set; } = string.Empty; // "Approved" or "Declined"
    }
}
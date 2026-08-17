namespace PRIV.DTOs
{
    public class UserSearchDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ConnectionStatus { get; set; } = "None"; // 'None', 'Pending', 'Approved', 'Declined'
    }
}
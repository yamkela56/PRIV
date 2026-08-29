namespace PRIV.DTOs
{
    // Shown in search results. Deliberately excludes any calendar/availability data.
    public class UserSearchResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ConnectionStatus { get; set; } = "None"; // None | Pending | Approved | Declined
    }
}

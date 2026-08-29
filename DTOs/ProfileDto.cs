namespace PRIV.DTOs
{
    // Shown on /u/username. Never includes calendar/busy details.
    public class ProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public bool IsSelf { get; set; }
        public string ConnectionStatus { get; set; } = "None"; // None | Pending | Approved | Declined


        // Only meaningful when IsSelf is true - lets the settings page pre-fill the toggle.
        public bool? DiscoverableInSearch { get; set; }
    }
}

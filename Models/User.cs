using System.ComponentModel.DataAnnotations;

namespace PRIV.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Display username, as the user typed it (preserves casing for display).
    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    // Lowercase, unique-indexed copy of Username. Used for all lookups and uniqueness checks so that "Yamkela" and "yamkela" collide (case-insensitive).
    [Required, MaxLength(50)]
    public string UsernameNormalized { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Email { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Bio { get; set; }

    // If true, the user's profile can be found via search. Even when false, a direct /u/username link still resolves the profile, but the user will not appear in search results.
    public bool DiscoverableInSearch { get; set; } = true;

    // Default working window used for available-slot calculation.
    public TimeSpan WorkDayStart { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan WorkDayEnd { get; set; } = new TimeSpan(20, 0, 0);
    public int SlotIncrementMinutes { get; set; } = 60;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BlockedTime> BlockedTimes { get; set; } = new List<BlockedTime>();
}
using System.ComponentModel.DataAnnotations;

namespace UserService.Models;

public class RefreshToken
{
    [Key]
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsUsed { get; set; } = false;

    public DateTime? UsedAt { get; set; } = null;

    [Required]
    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; } = null;
}

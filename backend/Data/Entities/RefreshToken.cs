using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_refresh_token")]
public class RefreshToken
{
    [Key]
    [Column("token_id")]
    public int TokenId { get; set; }
    
    [Column("user_id")]
    [Required]
    public int UserId { get; set; }

    [Column("token")]
    [Required]
    public string Token { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("expires_at")]
    [Required]
    public DateTimeOffset ExpiresAt { get; set; }

    [Column("revoked_at")]
    public DateTimeOffset? RevokedAt { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_users")]
public class User
{
    [Key]
    [Column("user_id")]
    public int UserId { set; get; }

    [MaxLength(50)]
    [Required]
    [Column("username")]
    public string Username { set; get; } = string.Empty;

    [MaxLength(255)]
    [Required]
    [EmailAddress]
    [Column("email")]
    public string Email { set; get; } = string.Empty;
    
    [MaxLength(255)]
    [Required]
    [Column("password")]
    public string Password { set; get; } = string.Empty;


    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set;}

    public ICollection<Review> Reviews { get; set;} = new List<Review>();
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_reviews")]
public class Review
{
    [Key]
    [Column("review_id")]
    public int ReviewId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("movie_id")]
    public int MovieId { get; set; }

    [MaxLength(50)]
    [Column("title")]
    public string? Title { get; set; }

    [Column("body")]
    public string? Body { get; set; }

    [Required]
    [Range(1, 10)]
    [Column("rating")]
    public int Rating { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set;}

    [ForeignKey("MovieId")]
    public Movie Movie { get; set; } = null!;

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_movies")]
public class Movie
{
    [Key]
    [Column("movie_id")]
    public int MovieId{ get; set; }

    [Required]
    [MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("poster_url")]
    public string PosterUrl { get; set; } = string.Empty;

    [Required]
    [Column("release_year")]
    public int ReleaseYear { get; set; }

    [Required]
    [Column("duration")]
    public int Duration { get; set; }

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set;}

    public ICollection<Review> Reviews { get; set; } = new List<Review>();  
}
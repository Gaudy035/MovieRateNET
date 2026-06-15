namespace backend.DTOs.Movies;

public class MovieResponseDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public int Duration { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public double? AverageRating { get; set; }
}
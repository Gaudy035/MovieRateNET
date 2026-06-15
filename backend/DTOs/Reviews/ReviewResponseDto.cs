namespace backend.DTOs.Reviews;

public class ReviewResponseDto
{
    public int ReviewId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int Rating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
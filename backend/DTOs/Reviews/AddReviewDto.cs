using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Reviews;

public class AddReviewDto
{
    public int? MovieId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }

    [Required]
    [Range(1, 10)]
    public int Rating { get; set; }
}
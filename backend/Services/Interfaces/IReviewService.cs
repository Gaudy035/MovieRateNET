using backend.DTOs.Reviews;

namespace backend.Services;

public interface IReviewService
{
    Task<IEnumerable<ReviewResponseDto>> GetReviewsByMovieId (int movieId);

    Task<bool> AddReview (int userId, int movieId, AddReviewDto dto);
}
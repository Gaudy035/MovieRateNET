using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs.Reviews;
using backend.Data.Entities;

namespace backend.Services;

public class ReviewService: IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByMovieId(int movieId)
    {
        return await _context.Reviews
            .Where(r => r.MovieId == movieId)
            .Select(r => new ReviewResponseDto
            {
                ReviewId = r.ReviewId,
                Username = r.User.Username,
                Title = r.Title,
                Body = r.Body,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt
            }
            ).ToListAsync();
        
    }

    public async Task<bool> AddReview(int userId, int movieId, AddReviewDto dto)
    {
        var movieExists = await _context.Movies
            .AnyAsync(m => m.MovieId == movieId);
        if (!movieExists)
        {
            return false;
        }

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.MovieId == movieId && r.UserId == userId);
        if (alreadyReviewed)
        {
            return false;
        }
        
        var newReview = new Review
        {
            UserId = userId,
            MovieId = movieId,
            Title = dto.Title,
            Body = dto.Body,
            Rating = dto.Rating
        };

        _context.Reviews.Add(newReview);
        await _context.SaveChangesAsync();
        return true;
    }
}
using backend.Data;
using backend.DTOs.Movies;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;
public class MovieService: IMovieService
{
    private readonly AppDbContext _context;
    
    public MovieService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MovieResponseDto>> GetAllMovies(string? titleFilter)
    {
        var query = _context.Movies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(titleFilter))
        {
            query = query.Where(m => EF.Functions.ILike(m.Title, $"%{titleFilter}%"));
        }
        
        return await query.Select(m => new MovieResponseDto
        {
            MovieId = m.MovieId,
            Title = m.Title,
            Description = m.Description,
            PosterUrl = m.PosterUrl,
            ReleaseYear = m.ReleaseYear,
            Duration = m.Duration,
            CreatedAt = m.CreatedAt,
            AverageRating = m.Reviews.Any() 
                ? m.Reviews.Average(r => r.Rating) 
                : null
        }).ToListAsync();
    }

    public async Task<MovieResponseDto?> GetMovieById(int movieId)
    {
        return await _context.Movies
            .Where(m => m.MovieId == movieId)
            .Select(m => new MovieResponseDto
            {
                MovieId = m.MovieId,
                Title = m.Title,
                Description = m.Description,
                PosterUrl = m.PosterUrl,
                ReleaseYear = m.ReleaseYear,
                Duration = m.Duration,
                CreatedAt = m.CreatedAt,
                AverageRating = m.Reviews.Any()
                    ? m.Reviews.Average(r => r.Rating)
                    : null
            }).FirstOrDefaultAsync();
    }
}
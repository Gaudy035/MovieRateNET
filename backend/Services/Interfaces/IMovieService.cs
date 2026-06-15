using backend.DTOs.Movies;

namespace backend.Services;

public interface IMovieService
{
    Task<IEnumerable<MovieResponseDto>> GetAllMovies(string? titleFilter);
    Task<MovieResponseDto?> GetMovieById(int movieId);
}
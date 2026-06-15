using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("movies")]
public class MovieController: ControllerBase
{
    private readonly IMovieService _movieService;

    public MovieController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMovies([FromQuery] string? title)
    {
        var movies = await _movieService.GetAllMovies(title);

        return Ok(movies);
    }

    [HttpGet("{movieId:int}")]
    public async Task<IActionResult> GetMovieById([FromRoute] int movieId)
    {
        var movie = await _movieService.GetMovieById(movieId);

        return Ok(movie);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using backend.DTOs.Reviews;
using backend.Services;
using System.Security.Claims;
using backend.DTOs;

namespace backend.Controllers;

[ApiController]
[Route("reviews")]
public class ReviewController: ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("{movieId:int}")]
    public async Task<IActionResult> GetReviewsByMovieId([FromRoute] int movieId)
    {
        var reviews = await _reviewService.GetReviewsByMovieId(movieId);
        return Ok(reviews);
    }

    [Authorize]
    [HttpPost("add/{movieId:int}")]
    public async Task<IActionResult> AddReview([FromRoute] int movieId, [FromBody] AddReviewDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userIdString.IsNullOrEmpty() || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized();
        }

        var success = await _reviewService.AddReview(userId, movieId, dto);

        return success
            ? Ok(new { message = "Recenzja dodana pomyslnie" })
            : BadRequest(new ErrorResponoseDto { StatusCode = 400, Message = "Oceniles juz ten film!" });
    }
}
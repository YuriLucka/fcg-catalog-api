using System.Security.Claims;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Registra uma avaliação para um jogo, em nome do usuário autenticado.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var review = await _reviewService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetByGame), new { gameId = review.GameId }, review);
        }

        /// <summary>
        /// Lista as avaliações de um jogo.
        /// </summary>
        [HttpGet("game/{gameId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByGame(Guid gameId)
        {
            var reviews = await _reviewService.GetByGameIdAsync(gameId);
            return Ok(reviews);
        }
    }
}

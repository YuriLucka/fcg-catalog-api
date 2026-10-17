using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Lista todos os jogos cadastrados na plataforma.
        /// </summary>
        /// <response code="200">Lista de jogos retornada com sucesso.</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var games = await _gameService.GetAllAsync();
            return Ok(games);
        }

        /// <summary>
        /// Busca jogos por título e descrição no Elasticsearch, tolerando erros de digitação.
        /// Resultados ordenados por relevância.
        /// </summary>
        /// <param name="q">Termo de busca.</param>
        /// <response code="200">Jogos encontrados, do mais relevante ao menos relevante.</response>
        /// <response code="400">Termo de busca vazio.</response>
        /// <response code="503">Serviço de busca indisponível.</response>
        [HttpGet("search")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Search([FromQuery] string? q)
        {
            var games = await _gameService.SearchAsync(q ?? string.Empty);
            return Ok(games);
        }

        /// <summary>
        /// Retorna os dados de um jogo pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador do jogo.</param>
        /// <response code="200">Jogo encontrado.</response>
        /// <response code="404">Jogo não encontrado.</response>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var game = await _gameService.GetByIdAsync(id);
            return Ok(game);
        }

        /// <summary>
        /// Cria um novo jogo na plataforma. Apenas administradores podem criar.
        /// </summary>
        /// <param name="dto">Dados para criação do jogo.</param>
        /// <response code="201">Jogo criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateGameDto dto)
        {
            var game = await _gameService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
        }

        /// <summary>
        /// Atualiza os dados de um jogo existente. Apenas administradores podem atualizar.
        /// </summary>
        /// <param name="id">Identificador do jogo.</param>
        /// <param name="dto">Novos dados do jogo.</param>
        /// <response code="200">Jogo atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameDto dto)
        {
            var game = await _gameService.UpdateAsync(id, dto);
            return Ok(game);
        }

        /// <summary>
        /// Remove um jogo da plataforma. Apenas administradores podem remover.
        /// </summary>
        /// <param name="id">Identificador do jogo.</param>
        /// <response code="204">Jogo removido com sucesso.</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _gameService.DeleteAsync(id);
            return NoContent();
        }
    }
}

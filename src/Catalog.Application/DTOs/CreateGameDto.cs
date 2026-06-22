namespace Catalog.Application.DTOs
{
    /// <summary>
    /// Dados para criação de um novo jogo.
    /// </summary>
    public record CreateGameDto(
        /// <summary>Título do jogo.</summary>
        string Title,
        /// <summary>Preço do jogo.</summary>
        decimal Price,
        /// <summary>Descrição do jogo.</summary>
        string? Description
    );
}

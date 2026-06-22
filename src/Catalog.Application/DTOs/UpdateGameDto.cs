namespace Catalog.Application.DTOs
{
    /// <summary>
    /// Dados para atualização de um jogo existente.
    /// </summary>
    public record UpdateGameDto(
        /// <summary>Título do jogo.</summary>
        string Title,
        /// <summary>Preço do jogo.</summary>
        decimal Price,
        /// <summary>Descrição do jogo.</summary>
        string? Description
    );
}

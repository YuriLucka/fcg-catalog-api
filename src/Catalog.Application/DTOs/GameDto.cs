namespace Catalog.Application.DTOs
{
    /// <summary>
    /// Dados de retorno de um jogo da plataforma.
    /// </summary>
    public record GameDto(
        /// <summary>Identificador único do jogo.</summary>
        Guid Id,
        /// <summary>Título do jogo.</summary>
        string Title,
        /// <summary>Descrição do jogo.</summary>
        string? Description,
        /// <summary>Preço do jogo.</summary>
        decimal Price,
        /// <summary>Indica se o jogo está ativo.</summary>
        bool IsActive,
        /// <summary>Data de criação do jogo.</summary>
        DateTime CreatedAt
    );
}

namespace Catalog.Application.DTOs
{
    public record CreateReviewDto(Guid GameId, int Rating, string? Comment);
}

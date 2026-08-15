namespace Catalog.Application.DTOs
{
    public record ReviewDto(string Id, Guid GameId, Guid UserId, int Rating, string? Comment, DateTime CreatedAt);
}

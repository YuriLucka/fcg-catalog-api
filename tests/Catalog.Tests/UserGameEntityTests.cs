using Catalog.Domain.Entities;
using Xunit;

namespace Catalog.Tests.Domain;

public class UserGameEntityTests
{
    [Fact]
    public void UserGame_is_created_with_user_and_game_and_new_id()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var ug = new UserGame(userId, gameId);

        Assert.Equal(userId, ug.UserId);
        Assert.Equal(gameId, ug.GameId);
        Assert.NotEqual(Guid.Empty, ug.Id);
    }
}

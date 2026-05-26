using System.Reflection;
using HouseOfChess.Platform.WebAPI.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace HouseOfChess.Platform.Tests.WebAPI.Controllers;

public class GameControllerAuthTests
{
    [Fact]
    public void Controller_RequiresAuthorization()
    {
        var authorize = typeof(GameController).GetCustomAttribute<AuthorizeAttribute>(inherit: true);
        Assert.NotNull(authorize);
    }

    [Theory]
    [InlineData(nameof(GameController.Ping))]
    [InlineData(nameof(GameController.GetById))]
    public void Endpoint_DoesNotAllowAnonymous(string methodName)
    {
        var method = typeof(GameController).GetMethod(methodName);
        Assert.NotNull(method);

        var anon = method!.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true);
        Assert.Null(anon);
    }
}

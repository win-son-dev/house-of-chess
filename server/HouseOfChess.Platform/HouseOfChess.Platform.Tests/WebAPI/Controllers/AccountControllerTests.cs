using System.Security.Claims;
using HouseOfChess.Platform.Infrastructure.Contracts.Account;
using HouseOfChess.Platform.Infrastructure.Services.Account;
using HouseOfChess.Platform.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace HouseOfChess.Platform.Tests.WebAPI.Controllers;

public class AccountControllerTests
{
    private const string Auth0Sub = "auth0|abc123";

    private static AccountController BuildSut(IAccountService accounts, string? sub = Auth0Sub)
    {
        var controller = new AccountController(accounts);
        var claims = sub is null
            ? Array.Empty<Claim>()
            : [new Claim(ClaimTypes.NameIdentifier, sub)];
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"))
            }
        };
        return controller;
    }

    [Fact]
    public async Task Onboarding_Created_Returns200_WithIsNewTrue()
    {
        var accounts = Substitute.For<IAccountService>();
        var id = Guid.NewGuid();
        accounts.OnboardAsync(Auth0Sub, "alice", Arg.Any<CancellationToken>())
                .Returns(OnboardingResult.Created(id, "alice"));

        var result = await BuildSut(accounts).Onboarding(new OnboardingRequest("alice"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<OnboardingResponse>(ok.Value);
        Assert.Equal(id, body.UserId);
        Assert.Equal("alice", body.Username);
        Assert.True(body.IsNew);
    }

    [Fact]
    public async Task Onboarding_AlreadyOnboarded_Returns200_WithIsNewFalse()
    {
        var accounts = Substitute.For<IAccountService>();
        var id = Guid.NewGuid();
        accounts.OnboardAsync(Auth0Sub, "ignored", Arg.Any<CancellationToken>())
                .Returns(OnboardingResult.AlreadyOnboarded(id, "bob"));

        var result = await BuildSut(accounts).Onboarding(new OnboardingRequest("ignored"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<OnboardingResponse>(ok.Value);
        Assert.Equal(id, body.UserId);
        Assert.Equal("bob", body.Username);
        Assert.False(body.IsNew);
    }

    [Fact]
    public async Task Onboarding_UsernameTaken_Returns409()
    {
        var accounts = Substitute.For<IAccountService>();
        accounts.OnboardAsync(Auth0Sub, "alice", Arg.Any<CancellationToken>())
                .Returns(OnboardingResult.UsernameTaken());

        var result = await BuildSut(accounts).Onboarding(new OnboardingRequest("alice"), CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task Onboarding_InvalidUsername_Returns400()
    {
        var accounts = Substitute.For<IAccountService>();
        accounts.OnboardAsync(Auth0Sub, "x", Arg.Any<CancellationToken>())
                .Returns(OnboardingResult.InvalidUsername("too short"));

        var result = await BuildSut(accounts).Onboarding(new OnboardingRequest("x"), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [Fact]
    public async Task Onboarding_NoSubClaim_Returns401_WithoutCallingService()
    {
        var accounts = Substitute.For<IAccountService>();

        var result = await BuildSut(accounts, sub: null).Onboarding(new OnboardingRequest("alice"), CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
        await accounts.DidNotReceive().OnboardAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}

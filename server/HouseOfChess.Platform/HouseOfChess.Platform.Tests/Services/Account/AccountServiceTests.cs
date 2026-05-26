using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Infrastructure.Services.Account;
using HouseOfChess.Platform.Services.Account;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace HouseOfChess.Platform.Tests.Services.Account;

public class AccountServiceTests
{
    private const string Sub = "auth0|abc123";

    private static AccountService Sut(IUserRepository users, UsernameOptions? opts = null) =>
        new(users, Options.Create(opts ?? new UsernameOptions()));

    [Fact]
    public async Task NewUser_HappyPath_ReturnsCreatedWithNewId()
    {
        var users = Substitute.For<IUserRepository>();
        users.GetByAuth0SubAsync(Sub, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);
        users.UsernameExistsAsync("alice", Arg.Any<CancellationToken>()).Returns(false);
        var id = Guid.NewGuid();
        users.CreateAsync(Sub, "alice", Arg.Any<CancellationToken>()).Returns(id);

        var result = await Sut(users).OnboardAsync(Sub, "alice", CancellationToken.None);

        Assert.Equal(OnboardingOutcome.Created, result.Outcome);
        Assert.Equal(id, result.UserId);
        Assert.Equal("alice", result.Username);
    }

    [Fact]
    public async Task ExistingAuth0Sub_ReturnsAlreadyOnboarded_AndDoesNotCreate()
    {
        var users = Substitute.For<IUserRepository>();
        var existing = new UserProfile(Guid.NewGuid(), "bob");
        users.GetByAuth0SubAsync(Sub, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await Sut(users).OnboardAsync(Sub, "ignored", CancellationToken.None);

        Assert.Equal(OnboardingOutcome.AlreadyOnboarded, result.Outcome);
        Assert.Equal(existing.Id, result.UserId);
        Assert.Equal("bob", result.Username);
        await users.DidNotReceive().UsernameExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await users.DidNotReceive().CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UsernameAlreadyTaken_ReturnsConflict_WithoutCallingCreate()
    {
        var users = Substitute.For<IUserRepository>();
        users.GetByAuth0SubAsync(Sub, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);
        users.UsernameExistsAsync("alice", Arg.Any<CancellationToken>()).Returns(true);

        var result = await Sut(users).OnboardAsync(Sub, "alice", CancellationToken.None);

        Assert.Equal(OnboardingOutcome.UsernameTaken, result.Outcome);
        await users.DidNotReceive().CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RaceCondition_CreateReturnsNull_TranslatesToUsernameTaken()
    {
        var users = Substitute.For<IUserRepository>();
        users.GetByAuth0SubAsync(Sub, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);
        users.UsernameExistsAsync("alice", Arg.Any<CancellationToken>()).Returns(false);
        users.CreateAsync(Sub, "alice", Arg.Any<CancellationToken>()).Returns((Guid?)null);

        var result = await Sut(users).OnboardAsync(Sub, "alice", CancellationToken.None);

        Assert.Equal(OnboardingOutcome.UsernameTaken, result.Outcome);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaa")] // 21 chars
    public async Task InvalidLength_ReturnsInvalidUsername(string username)
    {
        var users = Substitute.For<IUserRepository>();

        var result = await Sut(users).OnboardAsync(Sub, username, CancellationToken.None);

        Assert.Equal(OnboardingOutcome.InvalidUsername, result.Outcome);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
        await users.DidNotReceive().GetByAuth0SubAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("bad name")]   // space
    [InlineData("bad-name")]   // hyphen
    [InlineData("bad.name")]   // dot
    [InlineData("привет")]     // non-ASCII
    public async Task InvalidPattern_ReturnsInvalidUsername(string username)
    {
        var users = Substitute.For<IUserRepository>();

        var result = await Sut(users).OnboardAsync(Sub, username, CancellationToken.None);

        Assert.Equal(OnboardingOutcome.InvalidUsername, result.Outcome);
        await users.DidNotReceive().GetByAuth0SubAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UsernameIsTrimmedBeforeValidationAndPersist()
    {
        var users = Substitute.For<IUserRepository>();
        users.GetByAuth0SubAsync(Sub, Arg.Any<CancellationToken>()).Returns((UserProfile?)null);
        users.UsernameExistsAsync("alice", Arg.Any<CancellationToken>()).Returns(false);
        var id = Guid.NewGuid();
        users.CreateAsync(Sub, "alice", Arg.Any<CancellationToken>()).Returns(id);

        var result = await Sut(users).OnboardAsync(Sub, "  alice  ", CancellationToken.None);

        Assert.Equal(OnboardingOutcome.Created, result.Outcome);
        Assert.Equal("alice", result.Username);
        await users.Received(1).CreateAsync(Sub, "alice", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TighterOptionsAreRespected()
    {
        var users = Substitute.For<IUserRepository>();
        var opts = new UsernameOptions { MinLength = 5, MaxLength = 10, Pattern = "^[a-z]+$" };

        var tooShort = await Sut(users, opts).OnboardAsync(Sub, "abcd", CancellationToken.None);
        var badPattern = await Sut(users, opts).OnboardAsync(Sub, "Abcdef", CancellationToken.None);

        Assert.Equal(OnboardingOutcome.InvalidUsername, tooShort.Outcome);
        Assert.Equal(OnboardingOutcome.InvalidUsername, badPattern.Outcome);
    }
}

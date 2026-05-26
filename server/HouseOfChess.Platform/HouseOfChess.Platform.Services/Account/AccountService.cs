using System.Text.RegularExpressions;
using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Infrastructure.Services.Account;
using Microsoft.Extensions.Options;

namespace HouseOfChess.Platform.Services.Account;

public sealed class AccountService : IAccountService
{
    private readonly IUserRepository users;
    private readonly UsernameOptions usernameOptions;
    private readonly Regex usernamePattern;

    public AccountService(IUserRepository users, IOptions<UsernameOptions> usernameOptions)
    {
        this.users = users;
        this.usernameOptions = usernameOptions.Value;
        this.usernamePattern = new Regex(this.usernameOptions.Pattern, RegexOptions.Compiled);
    }

    public async Task<OnboardingResult> OnboardAsync(string auth0Sub, string username, CancellationToken ct = default)
    {
        var trimmed = username?.Trim() ?? string.Empty;

        var validationError = ValidateFormat(trimmed);
        if (validationError is not null)
            return OnboardingResult.InvalidUsername(validationError);

        var existing = await users.GetByAuth0SubAsync(auth0Sub, ct);
        if (existing is not null)
            return OnboardingResult.AlreadyOnboarded(existing.Id, existing.Username);

        if (await users.UsernameExistsAsync(trimmed, ct))
            return OnboardingResult.UsernameTaken();

        var newId = await users.CreateAsync(auth0Sub, trimmed, ct);
        return newId is null
            ? OnboardingResult.UsernameTaken()
            : OnboardingResult.Created(newId.Value, trimmed);
    }

    private string? ValidateFormat(string username)
    {
        if (username.Length < usernameOptions.MinLength || username.Length > usernameOptions.MaxLength)
            return $"Username must be between {usernameOptions.MinLength} and {usernameOptions.MaxLength} characters.";

        if (!usernamePattern.IsMatch(username))
            return "Username may only contain letters, digits and underscores.";

        return null;
    }
}

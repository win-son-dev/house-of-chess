namespace HouseOfChess.Platform.Infrastructure.Services.Account;

public enum OnboardingOutcome
{
    Created,
    AlreadyOnboarded,
    InvalidUsername,
    UsernameTaken
}

public sealed record OnboardingResult(
    OnboardingOutcome Outcome,
    Guid UserId,
    string Username,
    string? Error)
{
    public static OnboardingResult Created(Guid id, string username) =>
        new(OnboardingOutcome.Created, id, username, null);

    public static OnboardingResult AlreadyOnboarded(Guid id, string username) =>
        new(OnboardingOutcome.AlreadyOnboarded, id, username, null);

    public static OnboardingResult InvalidUsername(string reason) =>
        new(OnboardingOutcome.InvalidUsername, default, string.Empty, reason);

    public static OnboardingResult UsernameTaken() =>
        new(OnboardingOutcome.UsernameTaken, default, string.Empty, "Username already taken");
}

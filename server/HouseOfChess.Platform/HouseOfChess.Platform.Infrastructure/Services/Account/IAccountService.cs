namespace HouseOfChess.Platform.Infrastructure.Services.Account;

public interface IAccountService
{
    Task<OnboardingResult> OnboardAsync(string auth0Sub, string username, CancellationToken ct = default);
}

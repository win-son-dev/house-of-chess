namespace HouseOfChess.Platform.Infrastructure.Contracts.Account;

public sealed record OnboardingResponse(Guid UserId, string Username, bool IsNew);

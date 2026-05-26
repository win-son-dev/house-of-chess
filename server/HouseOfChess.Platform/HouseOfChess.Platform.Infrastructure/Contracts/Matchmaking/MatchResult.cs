namespace HouseOfChess.Platform.Infrastructure.Contracts.Matchmaking;

public sealed record MatchResult(
    bool Matched,
    Guid? GameId,
    Guid? WhiteUserId,
    Guid? BlackUserId,
    string? TimeControl);

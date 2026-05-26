namespace HouseOfChess.Platform.Infrastructure.Repositories;

public sealed record GameSummary(
    Guid Id,
    Guid WhiteUserId,
    Guid BlackUserId,
    string? Result,
    int MoveCount);

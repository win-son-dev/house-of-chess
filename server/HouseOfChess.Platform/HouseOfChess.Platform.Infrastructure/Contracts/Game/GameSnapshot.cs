namespace HouseOfChess.Platform.Infrastructure.Contracts.Game;

public sealed record GameSnapshot(
    Guid Id,
    Guid WhiteUserId,
    Guid BlackUserId,
    string TimeControl,
    string? Result,
    IReadOnlyList<GameMoveEntry> Moves);

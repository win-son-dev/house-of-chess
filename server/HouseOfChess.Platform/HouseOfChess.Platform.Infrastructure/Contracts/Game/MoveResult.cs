namespace HouseOfChess.Platform.Infrastructure.Contracts.Game;

public sealed record MoveResult(
    bool Accepted,
    string? RejectionReason,
    string? NewFen,
    string? San,
    string? Uci,
    GameResult? FinalResult);

namespace HouseOfChess.Platform.Packages.ChessEngine;

public sealed record MoveOutcome(
    bool Accepted,
    string? RejectionReason,
    string? NewFen,
    string? San,
    EngineGameResult? FinalResult);

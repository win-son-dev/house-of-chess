namespace HouseOfChess.Platform.Infrastructure.Contracts.PgnExport;

public sealed record PgnExportInputs(
    string WhiteUsername,
    string BlackUsername,
    string TimeControl,
    DateTime StartedAtUtc,
    string Result,
    IReadOnlyList<string> SanMoves);

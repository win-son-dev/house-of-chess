using Chess;

namespace HouseOfChess.Platform.Packages.ChessEngine;

public static class MoveEngine
{
    public static MoveOutcome Apply(string fen, string uci)
    {
        var parts = fen.Split('|');
        var cleanFen = parts[0];
        var initialFen = parts.Length > 1 ? parts[1] : cleanFen;
        var history = parts.Length > 2 
            ? parts[2].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() 
            : new List<string>();

        // Reconstruct the board by starting at initialFen and replaying all history
        if (!ChessBoard.TryLoadFromFen(initialFen, out var board, AutoEndgameRules.All))
            return new MoveOutcome(false, "Invalid FEN", null, null, null);

        foreach (var pastUci in history)
        {
            if (!TryApplyUciInternal(board, pastUci))
                return new MoveOutcome(false, "Corrupt history in FEN", null, null, null);
        }

        if (board.IsEndGame)
            return new MoveOutcome(false, "Game already ended", null, null, null);

        if (uci.Length is < 4 or > 5)
            return new MoveOutcome(false, "Invalid UCI length", null, null, null);

        if (!TryApplyUciInternal(board, uci, out var match))
            return new MoveOutcome(false, "Illegal move", null, null, null);

        EngineGameResult? finalResult = null;
        if (board.IsEndGame && board.EndGame is { } endgame)
        {
            finalResult = endgame.EndgameType switch
            {
                EndgameType.Checkmate or EndgameType.Resigned or EndgameType.Timeout =>
                    endgame.WonSide == PieceColor.White ? EngineGameResult.WhiteWin : EngineGameResult.BlackWin,
                _ => EngineGameResult.Draw
            };
        }

        history.Add(uci);
        var newFen = $"{board.ToFen()}|{initialFen}|{string.Join(",", history)}";

        return new MoveOutcome(true, null, newFen, match!.San, finalResult);
    }

    private static bool TryApplyUciInternal(ChessBoard board, string uci, out Move? match)
    {
        match = null;
        if (uci.Length is < 4 or > 5) return false;

        var from = uci[..2].ToLowerInvariant();
        var to = uci.Substring(2, 2).ToLowerInvariant();
        char? promoChar = uci.Length == 5 ? char.ToLowerInvariant(uci[4]) : null;

        foreach (var m in board.Moves(allowAmbiguousCastle: false, generateSan: true))
        {
            if (!string.Equals(m.OriginalPosition.ToString(), from, StringComparison.OrdinalIgnoreCase)) continue;
            if (!string.Equals(m.NewPosition.ToString(), to, StringComparison.OrdinalIgnoreCase)) continue;

            if (promoChar is not null)
            {
                if (!m.IsPromotion) continue;
                var promoPiece = m.Promotion?.ToString();
                if (string.IsNullOrEmpty(promoPiece)) continue;
                if (char.ToLowerInvariant(promoPiece[^1]) != promoChar.Value) continue;
            }
            else if (m.IsPromotion)
            {
                continue;
            }

            match = m;
            break;
        }

        if (match is null) return false;

        board.Move(match);
        return true;
    }

    private static bool TryApplyUciInternal(ChessBoard board, string uci)
    {
        return TryApplyUciInternal(board, uci, out _);
    }
}


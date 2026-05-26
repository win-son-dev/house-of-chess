using Chess;

namespace HouseOfChess.Platform.Packages.ChessEngine;

public static class MoveEngine
{
    public static MoveOutcome Apply(string fen, string uci)
    {
        if (!ChessBoard.TryLoadFromFen(fen, out var board, AutoEndgameRules.All))
            return new MoveOutcome(false, "Invalid FEN", null, null, null);

        if (board.IsEndGame)
            return new MoveOutcome(false, "Game already ended", null, null, null);

        if (uci.Length is < 4 or > 5)
            return new MoveOutcome(false, "Invalid UCI length", null, null, null);

        var from = uci[..2].ToLowerInvariant();
        var to = uci.Substring(2, 2).ToLowerInvariant();
        char? promoChar = uci.Length == 5 ? char.ToLowerInvariant(uci[4]) : null;

        Move? match = null;
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

        if (match is null)
            return new MoveOutcome(false, "Illegal move", null, null, null);

        board.Move(match);

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

        return new MoveOutcome(true, null, board.ToFen(), match.San, finalResult);
    }
}

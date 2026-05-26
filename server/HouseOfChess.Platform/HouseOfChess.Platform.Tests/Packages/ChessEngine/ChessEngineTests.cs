using Chess;
using HouseOfChess.Platform.Packages.ChessEngine;

namespace HouseOfChess.Platform.Tests.Packages.ChessEngine;

public class ChessEngineTests
{
    [Fact]
    public void StartingFenConstant_MatchesStandardInitialPosition()
    {
        Assert.Equal(
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            ChessConstants.StartingFen);
    }

    [Fact]
    public void GeraChess_LoadsStartingFen_AndRoundTripsCleanly()
    {
        var board = ChessBoard.LoadFromFen(ChessConstants.StartingFen);

        Assert.Equal(ChessConstants.StartingFen, board.ToFen());
    }

    [Fact]
    public void GeraChess_FromStartingPosition_Has20LegalMoves()
    {
        var board = new ChessBoard();

        Assert.Equal(20, board.Moves().Length);
    }
}

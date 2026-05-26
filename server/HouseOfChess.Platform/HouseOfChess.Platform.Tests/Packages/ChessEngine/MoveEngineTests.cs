using HouseOfChess.Platform.Packages.ChessEngine;

namespace HouseOfChess.Platform.Tests.Packages.ChessEngine;

public class MoveEngineTests
{
    [Fact]
    public void Apply_LegalOpeningMove_AcceptsAndReturnsNewFenAndSan()
    {
        var outcome = MoveEngine.Apply(ChessConstants.StartingFen, "e2e4");

        Assert.True(outcome.Accepted);
        Assert.Null(outcome.RejectionReason);
        Assert.Equal("e4", outcome.San);
        Assert.NotNull(outcome.NewFen);
        // Pawn moved to e4: rank-4 segment becomes "4P3", and side-to-move flips to black.
        Assert.Contains("4P3", outcome.NewFen);
        Assert.Equal("b", outcome.NewFen!.Split(' ')[1]);
        Assert.Null(outcome.FinalResult);
    }

    [Fact]
    public void Apply_IllegalMove_Rejects()
    {
        var outcome = MoveEngine.Apply(ChessConstants.StartingFen, "e2e5");

        Assert.False(outcome.Accepted);
        Assert.Equal("Illegal move", outcome.RejectionReason);
    }

    [Fact]
    public void Apply_InvalidFen_Rejects()
    {
        var outcome = MoveEngine.Apply("not a fen", "e2e4");

        Assert.False(outcome.Accepted);
        Assert.Equal("Invalid FEN", outcome.RejectionReason);
    }

    [Theory]
    [InlineData("e2")]
    [InlineData("e2e4e5")]
    public void Apply_BadUciLength_Rejects(string uci)
    {
        var outcome = MoveEngine.Apply(ChessConstants.StartingFen, uci);

        Assert.False(outcome.Accepted);
        Assert.Equal("Invalid UCI length", outcome.RejectionReason);
    }

    [Fact]
    public void Apply_FoolsMate_ReportsBlackWin()
    {
        // Fool's mate: 1. f3 e5 2. g4 Qh4#
        var fen1 = MoveEngine.Apply(ChessConstants.StartingFen, "f2f3").NewFen!;
        var fen2 = MoveEngine.Apply(fen1, "e7e5").NewFen!;
        var fen3 = MoveEngine.Apply(fen2, "g2g4").NewFen!;

        var mate = MoveEngine.Apply(fen3, "d8h4");

        Assert.True(mate.Accepted);
        Assert.Equal(EngineGameResult.BlackWin, mate.FinalResult);
    }

    [Fact]
    public void Apply_ScholarsMate_ReportsWhiteWin()
    {
        // Scholar's mate: 1. e4 e5 2. Bc4 Nc6 3. Qh5 Nf6?? 4. Qxf7#
        var fen1 = MoveEngine.Apply(ChessConstants.StartingFen, "e2e4").NewFen!;
        var fen2 = MoveEngine.Apply(fen1, "e7e5").NewFen!;
        var fen3 = MoveEngine.Apply(fen2, "f1c4").NewFen!;
        var fen4 = MoveEngine.Apply(fen3, "b8c6").NewFen!;
        var fen5 = MoveEngine.Apply(fen4, "d1h5").NewFen!;
        var fen6 = MoveEngine.Apply(fen5, "g8f6").NewFen!;

        var mate = MoveEngine.Apply(fen6, "h5f7");

        Assert.True(mate.Accepted);
        Assert.Equal(EngineGameResult.WhiteWin, mate.FinalResult);
    }

    [Fact]
    public void Apply_ThreefoldRepetition_ReportsDraw()
    {
        // Both sides shuffle knights between g1↔f3 / g8↔f6 until the same
        // played position has occurred three times (which happens on the 8th move, f6g8).
        string[] uciSequence =
        [
            "g1f3", "g8f6", "f3g1", "f6g8",
            "g1f3", "g8f6", "f3g1"
        ];
        var fen = ChessConstants.StartingFen;
        foreach (var uci in uciSequence)
        {
            var outcome = MoveEngine.Apply(fen, uci);
            Assert.True(outcome.Accepted, $"Failed to apply move {uci} on FEN '{fen}': {outcome.RejectionReason}");
            fen = outcome.NewFen!;
        }

        var thirdRepeat = MoveEngine.Apply(fen, "f6g8");

        Assert.True(thirdRepeat.Accepted);
        Assert.Equal(EngineGameResult.Draw, thirdRepeat.FinalResult);
    }



    [Fact]
    public void Apply_PromotionUci_AcceptsAndProducesPromotionSan()
    {
        // White pawn on a7, black king on e8 (out of the way), white king on h1.
        const string fen = "4k3/P7/8/8/8/8/8/7K w - - 0 1";

        var outcome = MoveEngine.Apply(fen, "a7a8q");

        Assert.True(outcome.Accepted);
        Assert.NotNull(outcome.San);
        Assert.Contains("=Q", outcome.San);
    }

    [Fact]
    public void Apply_PromotionMissingPromotionChar_Rejects()
    {
        // Same position; UCI without promotion letter is ambiguous → reject.
        const string fen = "4k3/P7/8/8/8/8/8/7K w - - 0 1";

        var outcome = MoveEngine.Apply(fen, "a7a8");

        Assert.False(outcome.Accepted);
    }
}

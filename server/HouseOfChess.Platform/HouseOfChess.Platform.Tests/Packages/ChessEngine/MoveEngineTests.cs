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

    [Fact]
    public void Apply_CastlingKingside_AcceptsAndRemovesRights()
    {
        // White can castle kingside (f1 and g1 are empty).
        const string fen = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1";
        
        var outcome = MoveEngine.Apply(fen, "e1g1");

        Assert.True(outcome.Accepted);
        Assert.Equal("O-O", outcome.San);
        Assert.NotNull(outcome.NewFen);
        // Castling rights should be updated (White can no longer castle, so KQ is removed from KQkq -> kq)
        var newFenParts = outcome.NewFen.Split('|')[0].Split(' ');
        Assert.Equal("kq", newFenParts[2]);
    }

    [Fact]
    public void Apply_CastlingThroughCheck_Rejects()
    {
        // Black rook at f8 attacks f1 square, so White cannot castle kingside through check.
        // Rank 8: r3kr1r (Rook on f8, King on e8)
        // Rank 7: ppppp1pp (f7 is empty)
        // Rank 2: ppppp1pp (f2 is empty)
        // Rank 1: R3K2R (King on e1, Rooks on a1 and h1)
        const string fenWithCheck = "r3kr1r/ppppp1pp/8/8/8/8/PPPPP1PP/R3K2R w KQkq - 0 1";

        var outcome = MoveEngine.Apply(fenWithCheck, "e1g1");

        Assert.False(outcome.Accepted);
        Assert.Equal("Illegal move", outcome.RejectionReason);
    }

    [Fact]
    public void Apply_CastlingAfterKingMoved_Rejects()
    {
        // White only has Q (queenside) castling right, no K (kingside) right.
        const string fenNoKingsideRight = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w Qkq - 0 1";
        
        var outcome = MoveEngine.Apply(fenNoKingsideRight, "e1g1");
        
        Assert.False(outcome.Accepted);
        Assert.Equal("Illegal move", outcome.RejectionReason);
    }

    [Fact]
    public void Apply_DoublePawnPush_SetsEnPassantTarget()
    {
        var outcome = MoveEngine.Apply(ChessConstants.StartingFen, "e2e4");

        Assert.True(outcome.Accepted);
        var newFenParts = outcome.NewFen!.Split('|')[0].Split(' ');
        Assert.Equal("e3", newFenParts[3]); // En passant target square should be e3
    }

    [Fact]
    public void Apply_EnPassantCapture_AcceptsAndRemovesPawn()
    {
        var fen = ChessConstants.StartingFen;
        fen = MoveEngine.Apply(fen, "e2e4").NewFen!;
        fen = MoveEngine.Apply(fen, "h7h6").NewFen!;
        fen = MoveEngine.Apply(fen, "e4e5").NewFen!;
        var pushOutcome = MoveEngine.Apply(fen, "d7d5");
        Assert.True(pushOutcome.Accepted);
        
        var captureOutcome = MoveEngine.Apply(pushOutcome.NewFen!, "e5d6");
        
        Assert.True(captureOutcome.Accepted);
        Assert.Equal("exd6", captureOutcome.San);
        
        var newFenParts = captureOutcome.NewFen!.Split('|')[0].Split(' ');
        // Target square should be empty (-)
        Assert.Equal("-", newFenParts[3]);
        // The board structure should have the captured pawn removed and the capturing pawn on d6
        Assert.StartsWith("rnbqkbnr/ppp1ppp1/3P3p/", newFenParts[0]);
    }

    [Fact]
    public void Apply_EnPassantLateCapture_Rejects()
    {
        var fen = ChessConstants.StartingFen;
        fen = MoveEngine.Apply(fen, "e2e4").NewFen!;
        fen = MoveEngine.Apply(fen, "h7h6").NewFen!;
        fen = MoveEngine.Apply(fen, "e4e5").NewFen!;
        var pushOutcome = MoveEngine.Apply(fen, "d7d5");
        Assert.True(pushOutcome.Accepted);
        
        // Instead of capturing immediately, White plays a quiet move: a2a3
        var quietOutcome = MoveEngine.Apply(pushOutcome.NewFen!, "a2a3");
        Assert.True(quietOutcome.Accepted);
        
        // Black plays a7a6
        var quietOutcome2 = MoveEngine.Apply(quietOutcome.NewFen!, "a7a6");
        Assert.True(quietOutcome2.Accepted);
        
        // Now White tries to capture en passant (e5d6) - should be rejected!
        var lateCaptureOutcome = MoveEngine.Apply(quietOutcome2.NewFen!, "e5d6");
        Assert.False(lateCaptureOutcome.Accepted);
        Assert.Equal("Illegal move", lateCaptureOutcome.RejectionReason);
    }

    [Fact]
    public void Apply_Stalemate_ReportsDraw()
    {
        // Black king on a8, White queen on c6, White king on h1.
        // White plays Qc6b6, reaching stalemate.
        const string beforeStalemate = "k7/8/2Q5/8/8/8/8/7K w - - 0 1";
        
        var outcome = MoveEngine.Apply(beforeStalemate, "c6b6");

        Assert.True(outcome.Accepted);
        Assert.Equal("Qb6$", outcome.San);
        Assert.Equal(EngineGameResult.Draw, outcome.FinalResult);
    }

    [Fact]
    public void Apply_InsufficientMaterial_ReportsDraw()
    {
        // Black King on a8, Black Pawn on g2, White King on h1.
        // White plays h1xg2 (King captures pawn), leaving only Kings -> Draw.
        const string fen = "k7/8/8/8/8/8/6p1/7K w - - 0 1";

        var outcome = MoveEngine.Apply(fen, "h1g2");

        Assert.True(outcome.Accepted);
        Assert.Equal("Kxg2", outcome.San);
        Assert.Equal(EngineGameResult.Draw, outcome.FinalResult);
    }

    [Fact]
    public void Apply_FiftyMoveRule_ReportsDraw()
    {
        // White King on h1, Black King on h8, White Rook on a2, Black Rook on a7.
        // FEN with halfmove clock at 99.
        const string fen = "7k/r7/8/8/8/8/R7/7K w - - 99 50";

        // White plays h1g1 (King moves, not a pawn move, not a capture).
        // This makes the halfmove clock 100 (50 full moves).
        var outcome = MoveEngine.Apply(fen, "h1g1");

        Assert.True(outcome.Accepted);
        Assert.Equal(EngineGameResult.Draw, outcome.FinalResult);
    }
}


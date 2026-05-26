using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Services.Rating;
using Microsoft.Extensions.Options;

namespace HouseOfChess.Platform.Tests.Services.Rating;

public class EloRatingServiceTests
{
    private static EloRatingService Sut(int kFactor = 20) =>
        new(Options.Create(new EloOptions { KFactor = kFactor }));

    [Fact]
    public void EqualRatedWhiteWin_AwardsHalfKToWhiteAndMinusHalfKToBlack()
    {
        var (w, b) = Sut().Compute(1500, 1500, GameResult.WhiteWin);

        Assert.Equal(1510, w);
        Assert.Equal(1490, b);
    }

    [Fact]
    public void EqualRatedDraw_KeepsRatingsUnchanged()
    {
        var (w, b) = Sut().Compute(1500, 1500, GameResult.Draw);

        Assert.Equal(1500, w);
        Assert.Equal(1500, b);
    }

    [Fact]
    public void EqualRatedBlackWin_IsMirrorOfWhiteWin()
    {
        var (w, b) = Sut().Compute(1500, 1500, GameResult.BlackWin);

        Assert.Equal(1490, w);
        Assert.Equal(1510, b);
    }

    [Fact]
    public void HigherRatedWin_ProducesSmallSwing()
    {
        var (w, b) = Sut().Compute(1800, 1500, GameResult.WhiteWin);

        var whiteGain = w - 1800;
        Assert.InRange(whiteGain, 1, 6);
        Assert.Equal(whiteGain, 1500 - b);
    }

    [Fact]
    public void LowerRatedUpset_ProducesLargeSwing()
    {
        var (w, b) = Sut().Compute(1500, 1800, GameResult.WhiteWin);

        var whiteGain = w - 1500;
        Assert.InRange(whiteGain, 14, 19);
        Assert.Equal(whiteGain, 1800 - b);
    }

    [Theory]
    [InlineData(GameResult.WhiteWin)]
    [InlineData(GameResult.BlackWin)]
    [InlineData(GameResult.Draw)]
    public void TotalRatingPointsAreConserved(GameResult result)
    {
        const int whiteBefore = 1600;
        const int blackBefore = 1400;

        var (w, b) = Sut().Compute(whiteBefore, blackBefore, result);

        Assert.Equal(whiteBefore + blackBefore, w + b);
    }

    [Fact]
    public void KFactorScalesTheSwingProportionally()
    {
        var (w20, _) = Sut(kFactor: 20).Compute(1500, 1500, GameResult.WhiteWin);
        var (w10, _) = Sut(kFactor: 10).Compute(1500, 1500, GameResult.WhiteWin);

        var swing20 = w20 - 1500;
        var swing10 = w10 - 1500;

        Assert.Equal(swing20, swing10 * 2);
    }
}

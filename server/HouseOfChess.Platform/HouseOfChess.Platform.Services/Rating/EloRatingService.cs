using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Infrastructure.Services.Rating;
using Microsoft.Extensions.Options;

namespace HouseOfChess.Platform.Services.Rating;

public sealed class EloRatingService(IOptions<EloOptions> options) : IRatingService
{
    private readonly EloOptions _options = options.Value;

    public (int NewWhite, int NewBlack) Compute(int whiteRating, int blackRating, GameResult result)
    {
        var (whiteScore, blackScore) = result switch
        {
            GameResult.WhiteWin => (1.0, 0.0),
            GameResult.BlackWin => (0.0, 1.0),
            GameResult.Draw     => (0.5, 0.5),
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };

        var expectedWhite = 1.0 / (1.0 + Math.Pow(10, (blackRating - whiteRating) / 400.0));
        var expectedBlack = 1.0 - expectedWhite;

        var k = _options.KFactor;
        var newWhite = (int)Math.Round(whiteRating + k * (whiteScore - expectedWhite));
        var newBlack = (int)Math.Round(blackRating + k * (blackScore - expectedBlack));

        return (newWhite, newBlack);
    }
}

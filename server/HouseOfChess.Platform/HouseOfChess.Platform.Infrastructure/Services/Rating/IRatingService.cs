using HouseOfChess.Platform.Infrastructure.Contracts.Game;

namespace HouseOfChess.Platform.Infrastructure.Services.Rating;

public interface IRatingService
{
    (int NewWhite, int NewBlack) Compute(int whiteRating, int blackRating, GameResult result);
}

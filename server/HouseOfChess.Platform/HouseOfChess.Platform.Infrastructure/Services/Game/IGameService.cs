using HouseOfChess.Platform.Infrastructure.Contracts.Game;

namespace HouseOfChess.Platform.Infrastructure.Services.Game;

public interface IGameService
{
    Task<MoveResult> SubmitMoveAsync(Guid gameId, Guid userId, MoveRequest move, CancellationToken ct = default);
    Task ResignAsync(Guid gameId, Guid userId, CancellationToken ct = default);
    Task<bool> OfferDrawAsync(Guid gameId, Guid userId, CancellationToken ct = default);
}

using HouseOfChess.Platform.Infrastructure.Contracts.Game;

namespace HouseOfChess.Platform.Infrastructure.Repositories;

public interface IMatchmakingQueueRepository
{
    Task EnqueueAsync(TimeControlCategory category, Guid userId, CancellationToken ct = default);
    Task<Guid?> TryDequeueAsync(TimeControlCategory category, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, CancellationToken ct = default);
}

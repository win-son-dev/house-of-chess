using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Contracts.Matchmaking;

namespace HouseOfChess.Platform.Infrastructure.Services.Matchmaking;

public interface IMatchmakingService
{
    Task<MatchResult> EnqueueAsync(Guid userId, TimeControlCategory category, CancellationToken ct = default);
    Task CancelAsync(Guid userId, CancellationToken ct = default);
}

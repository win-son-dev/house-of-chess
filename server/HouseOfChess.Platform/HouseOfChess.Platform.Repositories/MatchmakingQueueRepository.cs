using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Repositories;
using StackExchange.Redis;

namespace HouseOfChess.Platform.Repositories;

public sealed class MatchmakingQueueRepository(IConnectionMultiplexer redis) : IMatchmakingQueueRepository
{
    private IDatabase Db => redis.GetDatabase();
    private static string QueueKey(TimeControlCategory category) =>
        $"matchmaking:queue:{category.ToString().ToLowerInvariant()}";

    public Task EnqueueAsync(TimeControlCategory category, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Db.ListRightPushAsync(QueueKey(category), userId.ToString());
    }

    public async Task<Guid?> TryDequeueAsync(TimeControlCategory category, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var value = await Db.ListLeftPopAsync(QueueKey(category));
        return value.HasValue && Guid.TryParse(value.ToString(), out var id) ? id : null;
    }

    public async Task RemoveAsync(Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var marker = userId.ToString();
        foreach (TimeControlCategory category in Enum.GetValues(typeof(TimeControlCategory)))
        {
            await Db.ListRemoveAsync(QueueKey(category), marker);
        }
    }
}

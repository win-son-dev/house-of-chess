using HouseOfChess.Platform.Infrastructure.Repositories;
using StackExchange.Redis;

namespace HouseOfChess.Platform.Repositories;

public sealed class GameStateRepository(IConnectionMultiplexer redis) : IGameStateRepository
{
    private IDatabase Db => redis.GetDatabase();
    private static string FenKey(Guid gameId)    => $"game:{gameId}:fen";
    private static string ClocksKey(Guid gameId) => $"game:{gameId}:clocks";

    public async Task<string?> GetFenAsync(Guid gameId, CancellationToken ct = default)
    {
        var value = await Db.StringGetAsync(FenKey(gameId));
        return value.HasValue ? value.ToString() : null;
    }

    public Task SetFenAsync(Guid gameId, string fen, CancellationToken ct = default) =>
        Db.StringSetAsync(FenKey(gameId), fen);

    public async Task<(long WhiteMs, long BlackMs)?> GetClocksAsync(Guid gameId, CancellationToken ct = default)
    {
        var values = await Db.HashGetAsync(ClocksKey(gameId), new RedisValue[] { "white", "black" });
        if (!values[0].HasValue || !values[1].HasValue) return null;
        return ((long)values[0], (long)values[1]);
    }

    public Task SetClocksAsync(Guid gameId, long whiteMs, long blackMs, CancellationToken ct = default) =>
        Db.HashSetAsync(ClocksKey(gameId), new HashEntry[]
        {
            new("white", whiteMs),
            new("black", blackMs)
        });

    public Task ClearAsync(Guid gameId, CancellationToken ct = default) =>
        Db.KeyDeleteAsync(new RedisKey[] { FenKey(gameId), ClocksKey(gameId) });
}

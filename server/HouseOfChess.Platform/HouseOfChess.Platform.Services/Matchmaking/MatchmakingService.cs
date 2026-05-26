using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Contracts.Matchmaking;
using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Infrastructure.Services.Matchmaking;

namespace HouseOfChess.Platform.Services.Matchmaking;

public sealed class MatchmakingService(
    IMatchmakingQueueRepository queue,
    IGameRepository games) : IMatchmakingService
{
    public async Task<MatchResult> EnqueueAsync(Guid userId, TimeControlCategory category, CancellationToken ct = default)
    {
        var opponent = await queue.TryDequeueAsync(category, ct);

        if (opponent is null || opponent == userId)
        {
            await queue.EnqueueAsync(category, userId, ct);
            return new MatchResult(Matched: false, GameId: null, WhiteUserId: null, BlackUserId: null, TimeControl: null);
        }

        var callerIsWhite = Random.Shared.Next(2) == 0;
        var whiteUserId = callerIsWhite ? userId : opponent.Value;
        var blackUserId = callerIsWhite ? opponent.Value : userId;

        var timeControl = DefaultTimeControl(category);
        var gameId = await games.CreateAsync(whiteUserId, blackUserId, timeControl, ct);

        return new MatchResult(Matched: true, GameId: gameId, WhiteUserId: whiteUserId, BlackUserId: blackUserId, TimeControl: timeControl);
    }

    public Task CancelAsync(Guid userId, CancellationToken ct = default) =>
        queue.RemoveAsync(userId, ct);

    private static string DefaultTimeControl(TimeControlCategory c) => c switch
    {
        TimeControlCategory.Bullet => "1+0",
        TimeControlCategory.Blitz  => "5+0",
        TimeControlCategory.Rapid  => "10+0",
        _ => "5+0"
    };
}

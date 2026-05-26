using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Services.Matchmaking;
using NSubstitute;

namespace HouseOfChess.Platform.Tests.Services.Matchmaking;

public class MatchmakingServiceTests
{
    private static readonly Guid Caller = Guid.NewGuid();
    private static readonly Guid Opponent = Guid.NewGuid();

    private static (MatchmakingService sut, IMatchmakingQueueRepository queue, IGameRepository games) Build(Guid? dequeueResult)
    {
        var queue = Substitute.For<IMatchmakingQueueRepository>();
        var games = Substitute.For<IGameRepository>();
        queue.TryDequeueAsync(Arg.Any<TimeControlCategory>(), Arg.Any<CancellationToken>())
             .Returns(dequeueResult);
        games.CreateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(callInfo => Guid.NewGuid());
        return (new MatchmakingService(queue, games), queue, games);
    }

    [Fact]
    public async Task Enqueue_NoOpponent_EnqueuesSelfAndReturnsPending()
    {
        var (sut, queue, games) = Build(dequeueResult: null);

        var result = await sut.EnqueueAsync(Caller, TimeControlCategory.Blitz, CancellationToken.None);

        Assert.False(result.Matched);
        Assert.Null(result.GameId);
        await queue.Received(1).EnqueueAsync(TimeControlCategory.Blitz, Caller, Arg.Any<CancellationToken>());
        await games.DidNotReceive().CreateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enqueue_OpponentAvailable_CreatesGameAndReturnsMatched()
    {
        var (sut, queue, games) = Build(dequeueResult: Opponent);

        var result = await sut.EnqueueAsync(Caller, TimeControlCategory.Bullet, CancellationToken.None);

        Assert.True(result.Matched);
        Assert.NotNull(result.GameId);
        Assert.Equal("1+0", result.TimeControl);
        // Caller is either white or black; opponent has the other side.
        Assert.True(
            (result.WhiteUserId == Caller   && result.BlackUserId == Opponent) ||
            (result.WhiteUserId == Opponent && result.BlackUserId == Caller));
        await games.Received(1).CreateAsync(result.WhiteUserId!.Value, result.BlackUserId!.Value, "1+0", Arg.Any<CancellationToken>());
        await queue.DidNotReceive().EnqueueAsync(Arg.Any<TimeControlCategory>(), Caller, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Enqueue_DequeuesSelf_RequeuesAndReturnsPending()
    {
        // Edge case: the queue happened to contain the same user (double-enqueue).
        // Service should not match against self; should re-enqueue and report pending.
        var (sut, queue, games) = Build(dequeueResult: Caller);

        var result = await sut.EnqueueAsync(Caller, TimeControlCategory.Rapid, CancellationToken.None);

        Assert.False(result.Matched);
        await queue.Received(1).EnqueueAsync(TimeControlCategory.Rapid, Caller, Arg.Any<CancellationToken>());
        await games.DidNotReceive().CreateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancel_DelegatesToQueueRemove()
    {
        var (sut, queue, _) = Build(dequeueResult: null);

        await sut.CancelAsync(Caller, CancellationToken.None);

        await queue.Received(1).RemoveAsync(Caller, Arg.Any<CancellationToken>());
    }
}

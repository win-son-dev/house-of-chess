using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Services.Game;
using NSubstitute;

namespace HouseOfChess.Platform.Tests.Services.Game;

public class GameServiceTests
{
    private static readonly Guid White = Guid.NewGuid();
    private static readonly Guid Black = Guid.NewGuid();
    private static readonly Guid Stranger = Guid.NewGuid();
    private static readonly Guid GameId = Guid.NewGuid();

    private static (GameService sut, IGameRepository games, IGameStateRepository state) Build(
        GameSummary? summary,
        string? fen)
    {
        var games = Substitute.For<IGameRepository>();
        var state = Substitute.For<IGameStateRepository>();
        games.GetSummaryAsync(GameId, Arg.Any<CancellationToken>()).Returns(summary);
        state.GetFenAsync(GameId, Arg.Any<CancellationToken>()).Returns(fen);
        return (new GameService(games, state), games, state);
    }

    private static GameSummary FreshSummary(int moveCount = 0, string? result = null) =>
        new(GameId, White, Black, result, moveCount);

    [Fact]
    public async Task SubmitMove_GameNotFound_Rejects()
    {
        var (sut, _, _) = Build(summary: null, fen: null);

        var result = await sut.SubmitMoveAsync(GameId, White, new MoveRequest("e2e4"), CancellationToken.None);

        Assert.False(result.Accepted);
        Assert.Equal("Game not found", result.RejectionReason);
    }

    [Fact]
    public async Task SubmitMove_GameFinished_Rejects()
    {
        var (sut, _, _) = Build(summary: FreshSummary(result: "1-0"), fen: null);

        var result = await sut.SubmitMoveAsync(GameId, White, new MoveRequest("e2e4"), CancellationToken.None);

        Assert.False(result.Accepted);
        Assert.Equal("Game already finished", result.RejectionReason);
    }

    [Fact]
    public async Task SubmitMove_NotAPlayer_Rejects()
    {
        var (sut, _, _) = Build(summary: FreshSummary(), fen: null);

        var result = await sut.SubmitMoveAsync(GameId, Stranger, new MoveRequest("e2e4"), CancellationToken.None);

        Assert.False(result.Accepted);
        Assert.Equal("Not a player in this game", result.RejectionReason);
    }

    [Fact]
    public async Task SubmitMove_NotYourTurn_Rejects()
    {
        // Fresh game: white to move; black tries to move.
        var (sut, _, _) = Build(summary: FreshSummary(), fen: null);

        var result = await sut.SubmitMoveAsync(GameId, Black, new MoveRequest("e7e5"), CancellationToken.None);

        Assert.False(result.Accepted);
        Assert.Equal("Not your turn", result.RejectionReason);
    }

    [Fact]
    public async Task SubmitMove_IllegalMove_RejectsWithoutPersisting()
    {
        var (sut, games, state) = Build(summary: FreshSummary(), fen: null);

        var result = await sut.SubmitMoveAsync(GameId, White, new MoveRequest("e2e5"), CancellationToken.None);

        Assert.False(result.Accepted);
        await games.DidNotReceive().AppendMoveAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await state.DidNotReceive().SetFenAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitMove_ValidOpeningMove_PersistsAndUpdatesCache()
    {
        var (sut, games, state) = Build(summary: FreshSummary(), fen: null);

        var result = await sut.SubmitMoveAsync(GameId, White, new MoveRequest("e2e4"), CancellationToken.None);

        Assert.True(result.Accepted);
        Assert.Equal("e4", result.San);
        Assert.Equal("e2e4", result.Uci);
        Assert.NotNull(result.NewFen);
        Assert.Null(result.FinalResult);
        await games.Received(1).AppendMoveAsync(GameId, 1, "e4", "e2e4", Arg.Any<CancellationToken>());
        await state.Received(1).SetFenAsync(GameId, result.NewFen!, Arg.Any<CancellationToken>());
        await games.DidNotReceive().FinishAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitMove_CheckmateMove_PersistsAndFinishesGame()
    {
        // Fool's mate: 1. f3 e5 2. g4 Qh4#. We start one move before mate.
        const string beforeMate = "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq g3 0 2";
        var (sut, games, state) = Build(summary: FreshSummary(moveCount: 3), fen: beforeMate);

        var result = await sut.SubmitMoveAsync(GameId, Black, new MoveRequest("d8h4"), CancellationToken.None);

        Assert.True(result.Accepted);
        Assert.Equal(GameResult.BlackWin, result.FinalResult);
        await games.Received(1).AppendMoveAsync(GameId, 4, Arg.Any<string>(), "d8h4", Arg.Any<CancellationToken>());
        await games.Received(1).FinishAsync(GameId, "0-1", Arg.Any<CancellationToken>());
    }
}

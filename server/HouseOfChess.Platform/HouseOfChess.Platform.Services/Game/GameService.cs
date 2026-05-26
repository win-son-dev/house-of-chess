using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Infrastructure.Services.Game;
using HouseOfChess.Platform.Packages.ChessEngine;

namespace HouseOfChess.Platform.Services.Game;

public sealed class GameService(
    IGameRepository games,
    IGameStateRepository state) : IGameService
{
    public async Task<MoveResult> SubmitMoveAsync(Guid gameId, Guid userId, MoveRequest move, CancellationToken ct = default)
    {
        var summary = await games.GetSummaryAsync(gameId, ct);
        if (summary is null)            return Reject("Game not found");
        if (summary.Result is not null) return Reject("Game already finished");
        if (userId != summary.WhiteUserId && userId != summary.BlackUserId)
            return Reject("Not a player in this game");

        var fen = await state.GetFenAsync(gameId, ct) ?? ChessConstants.StartingFen;

        var sideToMove = fen.Split(' ')[1];
        var expectedUserId = sideToMove == "w" ? summary.WhiteUserId : summary.BlackUserId;
        if (userId != expectedUserId) return Reject("Not your turn");

        var outcome = MoveEngine.Apply(fen, move.Uci);
        if (!outcome.Accepted)
            return new MoveResult(false, outcome.RejectionReason, null, null, null, null);

        var newPly = summary.MoveCount + 1;
        await games.AppendMoveAsync(gameId, newPly, outcome.San!, move.Uci, ct);
        await state.SetFenAsync(gameId, outcome.NewFen!, ct);

        var finalResult = MapResult(outcome.FinalResult);
        if (finalResult.HasValue)
        {
            await games.FinishAsync(gameId, ResultToken(finalResult.Value), ct);
        }

        return new MoveResult(true, null, outcome.NewFen, outcome.San, move.Uci, finalResult);
    }

    public Task ResignAsync(Guid gameId, Guid userId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<bool> OfferDrawAsync(Guid gameId, Guid userId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    private static MoveResult Reject(string reason) =>
        new(false, reason, null, null, null, null);

    private static GameResult? MapResult(EngineGameResult? r) => r switch
    {
        EngineGameResult.WhiteWin => GameResult.WhiteWin,
        EngineGameResult.BlackWin => GameResult.BlackWin,
        EngineGameResult.Draw     => GameResult.Draw,
        _                         => null
    };

    private static string ResultToken(GameResult r) => r switch
    {
        GameResult.WhiteWin => "1-0",
        GameResult.BlackWin => "0-1",
        GameResult.Draw     => "1/2-1/2",
        _                   => "*"
    };
}

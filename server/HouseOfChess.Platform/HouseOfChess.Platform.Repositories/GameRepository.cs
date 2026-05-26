using System.Text;
using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseOfChess.Platform.Repositories;

public sealed class GameRepository(ApplicationDbContext db) : IGameRepository
{
    public async Task<Guid> CreateAsync(Guid whiteUserId, Guid blackUserId, string timeControl, CancellationToken ct = default)
    {
        var game = new Game
        {
            WhiteUserId = whiteUserId,
            BlackUserId = blackUserId,
            TimeControl = timeControl
        };
        db.Games.Add(game);
        await db.SaveChangesAsync(ct);
        return game.Id;
    }

    public Task<GameSummary?> GetSummaryAsync(Guid gameId, CancellationToken ct = default) =>
        db.Games
            .Where(g => g.Id == gameId)
            .Select(g => new GameSummary(
                g.Id,
                g.WhiteUserId,
                g.BlackUserId,
                g.Result,
                db.GameMoves.Count(m => m.GameId == g.Id)))
            .SingleOrDefaultAsync(ct);

    public async Task<GameSnapshot?> GetSnapshotAsync(Guid gameId, CancellationToken ct = default)
    {
        var game = await db.Games
            .Where(g => g.Id == gameId)
            .Select(g => new { g.Id, g.WhiteUserId, g.BlackUserId, g.TimeControl, g.Result })
            .SingleOrDefaultAsync(ct);
        if (game is null) return null;

        var moves = await db.GameMoves
            .Where(m => m.GameId == gameId)
            .OrderBy(m => m.Ply)
            .Select(m => new GameMoveEntry(m.Ply, m.San, m.Uci))
            .ToListAsync(ct);

        return new GameSnapshot(game.Id, game.WhiteUserId, game.BlackUserId, game.TimeControl, game.Result, moves);
    }

    public async Task AppendMoveAsync(Guid gameId, int ply, string san, string uci, CancellationToken ct = default)
    {
        db.GameMoves.Add(new GameMove { GameId = gameId, Ply = ply, San = san, Uci = uci });
        await db.SaveChangesAsync(ct);
    }

    public async Task FinishAsync(Guid gameId, string result, CancellationToken ct = default)
    {
        var game = await db.Games.FirstOrDefaultAsync(g => g.Id == gameId, ct)
            ?? throw new InvalidOperationException($"Game {gameId} not found");

        var sans = await db.GameMoves
            .Where(m => m.GameId == gameId)
            .OrderBy(m => m.Ply)
            .Select(m => m.San)
            .ToListAsync(ct);

        game.Result = result;
        game.Pgn = BuildPgn(sans, result);
        game.EndedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private static string BuildPgn(IReadOnlyList<string> sans, string result)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < sans.Count; i++)
        {
            if (i % 2 == 0) sb.Append((i / 2) + 1).Append(". ");
            sb.Append(sans[i]).Append(' ');
        }
        sb.Append(result);
        return sb.ToString();
    }
}

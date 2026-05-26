using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Contracts.PgnExport;

namespace HouseOfChess.Platform.Infrastructure.Repositories;

public interface IGameRepository
{
    Task<Guid> CreateAsync(Guid whiteUserId, Guid blackUserId, string timeControl, CancellationToken ct = default);
    Task<GameSummary?> GetSummaryAsync(Guid gameId, CancellationToken ct = default);
    Task<GameSnapshot?> GetSnapshotAsync(Guid gameId, CancellationToken ct = default);
    Task<PgnExportInputs?> GetPgnExportInputsAsync(Guid gameId, string result, CancellationToken ct = default);
    Task AppendMoveAsync(Guid gameId, int ply, string san, string uci, CancellationToken ct = default);
    Task FinishAsync(Guid gameId, string result, string pgn, CancellationToken ct = default);
}

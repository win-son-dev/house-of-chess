using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Infrastructure.Services.AntiCheat;
using HouseOfChess.Platform.Infrastructure.Services.Engine;

namespace HouseOfChess.Platform.Services.AntiCheat;

public sealed class AntiCheatService(IStockfishService stockfish, IGameRepository games) : IAntiCheatService
{
    public Task AnalyzeFinishedGameAsync(Guid gameId, CancellationToken ct = default) =>
        throw new NotImplementedException();
}

using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Infrastructure.Services.Engine;
using Microsoft.Extensions.Options;

namespace HouseOfChess.Platform.Services.Engine;

public sealed class StockfishService(IOptions<StockfishOptions> options) : IStockfishService
{
    private readonly StockfishOptions _options = options.Value;

    public Task<int> EvaluateCentipawnsAsync(string fen, int depth, CancellationToken ct = default) =>
        throw new NotImplementedException($"Stockfish at {_options.Path} not yet wired");

    public Task<string> BestMoveAsync(string fen, int depth, CancellationToken ct = default) =>
        throw new NotImplementedException($"Stockfish at {_options.Path} not yet wired");
}

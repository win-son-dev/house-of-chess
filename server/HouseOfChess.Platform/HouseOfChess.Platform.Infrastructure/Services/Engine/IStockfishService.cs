namespace HouseOfChess.Platform.Infrastructure.Services.Engine;

public interface IStockfishService
{
    Task<int> EvaluateCentipawnsAsync(string fen, int depth, CancellationToken ct = default);
    Task<string> BestMoveAsync(string fen, int depth, CancellationToken ct = default);
}

namespace HouseOfChess.Platform.Infrastructure.Repositories;

public interface IGameStateRepository
{
    Task<string?> GetFenAsync(Guid gameId, CancellationToken ct = default);
    Task SetFenAsync(Guid gameId, string fen, CancellationToken ct = default);
    Task<(long WhiteMs, long BlackMs)?> GetClocksAsync(Guid gameId, CancellationToken ct = default);
    Task SetClocksAsync(Guid gameId, long whiteMs, long blackMs, CancellationToken ct = default);
    Task ClearAsync(Guid gameId, CancellationToken ct = default);
}

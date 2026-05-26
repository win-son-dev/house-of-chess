namespace HouseOfChess.Platform.Infrastructure.Services.AntiCheat;

public interface IAntiCheatService
{
    Task AnalyzeFinishedGameAsync(Guid gameId, CancellationToken ct = default);
}

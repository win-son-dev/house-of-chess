namespace HouseOfChess.Platform.Infrastructure.Options;

public sealed class StockfishOptions
{
    public string Path { get; init; } = "/usr/games/stockfish";
    public int Depth { get; init; } = 18;
}

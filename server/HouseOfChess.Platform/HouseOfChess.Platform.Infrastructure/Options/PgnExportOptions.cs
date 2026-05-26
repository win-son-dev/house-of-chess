namespace HouseOfChess.Platform.Infrastructure.Options;

public sealed class PgnExportOptions
{
    public string Event { get; init; } = "House of Chess";
    public string Site { get; init; } = "House of Chess";
    public string Round { get; init; } = "-";
}

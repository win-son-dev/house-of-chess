namespace HouseOfChess.Platform.Infrastructure.Options;

public sealed class ConnectionStringsOptions
{
    public string Postgres { get; init; } = string.Empty;
    public string Redis { get; init; } = string.Empty;
}

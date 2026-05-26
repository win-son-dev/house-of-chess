namespace HouseOfChess.Platform.Infrastructure.Options;

public sealed class CorsOptions
{
    public string[] AllowedOrigins { get; init; } = [];
}

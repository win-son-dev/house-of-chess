namespace HouseOfChess.Platform.Infrastructure.Options;

public sealed class Auth0Options
{
    public string Domain { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}

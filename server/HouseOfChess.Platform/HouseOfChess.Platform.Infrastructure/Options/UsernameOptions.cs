namespace HouseOfChess.Platform.Infrastructure.Options;

public sealed class UsernameOptions
{
    public int MinLength { get; init; } = 3;
    public int MaxLength { get; init; } = 20;
    public string Pattern { get; init; } = "^[a-zA-Z0-9_]+$";
}

namespace HouseOfChess.Platform.Repositories.Entities;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WhiteUserId { get; set; }
    public Guid BlackUserId { get; set; }
    public required string TimeControl { get; set; }
    public string Pgn { get; set; } = string.Empty;
    public string? Result { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
}

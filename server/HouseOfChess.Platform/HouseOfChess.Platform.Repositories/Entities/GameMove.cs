namespace HouseOfChess.Platform.Repositories.Entities;

public class GameMove
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GameId { get; set; }
    public int Ply { get; set; }
    public required string San { get; set; }
    public required string Uci { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}

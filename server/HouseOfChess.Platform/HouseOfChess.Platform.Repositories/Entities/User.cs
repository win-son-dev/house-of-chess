namespace HouseOfChess.Platform.Repositories.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Auth0Sub { get; set; }
    public required string Username { get; set; }
    public int EloBullet { get; set; } = 1500;
    public int EloBlitz { get; set; } = 1500;
    public int EloRapid { get; set; } = 1500;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

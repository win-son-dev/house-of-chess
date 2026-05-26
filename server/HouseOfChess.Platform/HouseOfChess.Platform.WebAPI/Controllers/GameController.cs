using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseOfChess.Platform.WebAPI.Controllers;

[ApiController]
[Route("api/games")]
[Authorize]
public sealed class GameController(IGameRepository games) : ControllerBase
{
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new { ok = true });

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GameSnapshot>> GetById(Guid id, CancellationToken ct)
    {
        var snapshot = await games.GetSnapshotAsync(id, ct);
        return snapshot is null ? NotFound() : Ok(snapshot);
    }
}

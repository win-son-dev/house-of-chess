using HouseOfChess.Platform.Infrastructure.Contracts.Game;
using HouseOfChess.Platform.Infrastructure.Contracts.Matchmaking;
using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Infrastructure.Services.Game;
using HouseOfChess.Platform.Infrastructure.Services.Matchmaking;
using HouseOfChess.Platform.WebAPI.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HouseOfChess.Platform.WebAPI.Hubs;

[Authorize]
public sealed class GameHub(
    IUserRepository users,
    IMatchmakingService matchmaking,
    IGameService gameService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var sub = Context.User?.GetAuth0Sub();
        if (string.IsNullOrEmpty(sub))
        {
            Context.Abort();
            return;
        }

        var profile = await users.GetByAuth0SubAsync(sub, Context.ConnectionAborted);
        if (profile is null)
        {
            Context.Abort();
            return;
        }

        Context.Items["userId"] = profile.Id;
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroupName(profile.Id));
        await base.OnConnectedAsync();
    }

    public Task JoinGame(string gameId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GameGroupName(gameId));

    public Task LeaveGame(string gameId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GameGroupName(gameId));

    public async Task<MatchResult> EnqueueMatch(TimeControlCategory category)
    {
        var userId = RequireUserId();
        var result = await matchmaking.EnqueueAsync(userId, category, Context.ConnectionAborted);

        if (result.Matched)
        {
            await Clients
                .Groups(UserGroupName(result.WhiteUserId!.Value), UserGroupName(result.BlackUserId!.Value))
                .SendAsync("matchFound", result, Context.ConnectionAborted);
        }

        return result;
    }

    public Task CancelMatch() =>
        matchmaking.CancelAsync(RequireUserId(), Context.ConnectionAborted);

    public async Task<MoveResult> SubmitMove(string gameId, MoveRequest move)
    {
        if (!Guid.TryParse(gameId, out var gid))
            throw new HubException("Invalid gameId");

        var userId = RequireUserId();
        var result = await gameService.SubmitMoveAsync(gid, userId, move, Context.ConnectionAborted);

        if (result.Accepted)
        {
            await Clients.Group(GameGroupName(gameId))
                .SendAsync("moveApplied", result, Context.ConnectionAborted);
        }

        return result;
    }

    private Guid RequireUserId() =>
        Context.Items["userId"] is Guid id
            ? id
            : throw new HubException("Connection not initialized");

    internal static string GameGroupName(string gameId) => $"game:{gameId}";
    internal static string UserGroupName(Guid userId) => $"user:{userId}";
}

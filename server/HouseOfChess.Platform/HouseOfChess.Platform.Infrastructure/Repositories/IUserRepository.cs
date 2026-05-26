namespace HouseOfChess.Platform.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<UserProfile?> GetByAuth0SubAsync(string auth0Sub, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);

    /// <summary>Returns the new user id, or null if the username was taken by a concurrent insert.</summary>
    Task<Guid?> CreateAsync(string auth0Sub, string username, CancellationToken ct = default);
}

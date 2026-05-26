using HouseOfChess.Platform.Infrastructure.Repositories;
using HouseOfChess.Platform.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseOfChess.Platform.Repositories;

public sealed class UserRepository(ApplicationDbContext db) : IUserRepository
{
    private const string PostgresUniqueViolation = "23505";
    private const string UsernameIndex = "IX_Users_Username";

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        db.Users.AnyAsync(u => u.Id == id, ct);

    public Task<UserProfile?> GetByAuth0SubAsync(string auth0Sub, CancellationToken ct = default) =>
        db.Users
            .Where(u => u.Auth0Sub == auth0Sub)
            .Select(u => new UserProfile(u.Id, u.Username))
            .SingleOrDefaultAsync(ct);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default) =>
        db.Users.AnyAsync(u => u.Username == username, ct);

    public async Task<Guid?> CreateAsync(string auth0Sub, string username, CancellationToken ct = default)
    {
        var user = new User { Auth0Sub = auth0Sub, Username = username };
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
            return user.Id;
        }
        catch (DbUpdateException ex) when (IsUsernameConflict(ex))
        {
            db.Entry(user).State = EntityState.Detached;
            return null;
        }
    }

    private static bool IsUsernameConflict(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg
        && pg.SqlState == PostgresUniqueViolation
        && pg.ConstraintName == UsernameIndex;
}

using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HouseOfChess.Platform.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformRepositories(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>((sp, opt) =>
        {
            var conn = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
            opt.UseNpgsql(conn.Postgres);
        });

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var conn = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
            return ConnectionMultiplexer.Connect(conn.Redis);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddSingleton<IGameStateRepository, GameStateRepository>();
        services.AddSingleton<IMatchmakingQueueRepository, MatchmakingQueueRepository>();

        return services;
    }
}

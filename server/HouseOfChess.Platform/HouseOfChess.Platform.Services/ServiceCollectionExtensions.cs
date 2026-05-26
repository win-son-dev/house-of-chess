using HouseOfChess.Platform.Infrastructure.Constants;
using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Infrastructure.Services.Account;
using HouseOfChess.Platform.Infrastructure.Services.AntiCheat;
using HouseOfChess.Platform.Infrastructure.Services.Engine;
using HouseOfChess.Platform.Infrastructure.Services.Game;
using HouseOfChess.Platform.Infrastructure.Services.Matchmaking;
using HouseOfChess.Platform.Infrastructure.Services.PgnExport;
using HouseOfChess.Platform.Infrastructure.Services.Rating;
using HouseOfChess.Platform.Services.Account;
using HouseOfChess.Platform.Services.AntiCheat;
using HouseOfChess.Platform.Services.Engine;
using HouseOfChess.Platform.Services.Game;
using HouseOfChess.Platform.Services.Matchmaking;
using HouseOfChess.Platform.Services.PgnExport;
using HouseOfChess.Platform.Services.Rating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HouseOfChess.Platform.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<EloOptions>()
            .Bind(configuration.GetSection(ConfigSections.Elo))
            .ValidateOnStart();

        services.AddOptions<StockfishOptions>()
            .Bind(configuration.GetSection(ConfigSections.Stockfish))
            .ValidateOnStart();

        services.AddOptions<Auth0Options>()
            .Bind(configuration.GetSection(ConfigSections.Auth0))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Domain),   "Auth0.Domain not set")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Auth0.Audience not set")
            .ValidateOnStart();

        services.AddOptions<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConfigSections.ConnectionStrings))
            .Validate(c => !string.IsNullOrWhiteSpace(c.Postgres), "ConnectionStrings.Postgres not set")
            .Validate(c => !string.IsNullOrWhiteSpace(c.Redis),    "ConnectionStrings.Redis not set")
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(ConfigSections.Cors))
            .ValidateOnStart();

        services.AddOptions<UsernameOptions>()
            .Bind(configuration.GetSection(ConfigSections.Username))
            .Validate(o => o.MinLength > 0 && o.MaxLength >= o.MinLength, "Username MinLength/MaxLength invalid")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Pattern), "Username Pattern not set")
            .ValidateOnStart();

        services.AddOptions<PgnExportOptions>()
            .Bind(configuration.GetSection(ConfigSections.PgnExport))
            .ValidateOnStart();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IGameService, GameService>();
        services.AddSingleton<IMatchmakingService, MatchmakingService>();
        services.AddSingleton<IRatingService, EloRatingService>();
        services.AddScoped<IAntiCheatService, AntiCheatService>();
        services.AddSingleton<IStockfishService, StockfishService>();
        services.AddSingleton<IPgnExportService, PgnExportService>();

        return services;
    }
}

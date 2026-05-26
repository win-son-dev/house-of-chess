using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Repositories;
using HouseOfChess.Platform.Services;
using HouseOfChess.Platform.WebAPI.ExceptionHandlers;
using HouseOfChess.Platform.WebAPI.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using AspCorsOptions = Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPlatformServices(builder.Configuration);
builder.Services.AddPlatformRepositories();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<Auth0Options>>((jwt, auth0Opt) =>
    {
        var auth0 = auth0Opt.Value;
        jwt.Authority = $"https://{auth0.Domain}/";
        jwt.Audience = auth0.Audience;
        jwt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://{auth0.Domain}/",
            ValidateAudience = true,
            ValidAudience = auth0.Audience,
            ValidateLifetime = true
        };
        jwt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token)
                    && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSignalR().AddStackExchangeRedis();
builder.Services.AddOptions<RedisOptions>()
    .Configure<IOptions<ConnectionStringsOptions>>((redis, connOpt) =>
    {
        redis.Configuration = ConfigurationOptions.Parse(connOpt.Value.Redis);
        redis.Configuration.ChannelPrefix = RedisChannel.Literal("houseofchess");
    });

builder.Services.AddCors();
builder.Services.AddOptions<AspCorsOptions>()
    .Configure<IOptions<CorsOptions>>((mvcCors, ourCors) =>
    {
        mvcCors.AddDefaultPolicy(p => p
            .WithOrigins(ourCors.Value.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddNpgSql(sp => sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Postgres, name: "postgres")
    .AddRedis(sp => sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Redis, name: "redis");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/hubs/game");
app.MapHealthChecks("/health");

app.Run();

using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Voting.Storage;
using Worker.Services;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(key: "Redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
  var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;

  return string.IsNullOrWhiteSpace(options.ConnectionString)
    ? throw new InvalidOperationException(message: "Redis connection string is not configured.")
    : ConnectionMultiplexer.Connect(options.ConnectionString);
});

builder.Services.AddDbContextFactory<VoteContext>();

builder.Services.AddHealthChecks();
builder.Services.AddHostedService<PollingService>();

var app = builder.Build();

app.MapHealthChecks(pattern: "/healthz");
app.MapGet(pattern: "/", () => Results.Ok(new { Status = "Worker ready" }));

app.Run();
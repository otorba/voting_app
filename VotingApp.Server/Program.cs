using System.Globalization;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VotingApp.Server.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(key: "Redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
  var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;

  if (string.IsNullOrWhiteSpace(options.ConnectionString))
    throw new InvalidOperationException(message: "Redis connection string is not configured.");

  return ConnectionMultiplexer.Connect(options.ConnectionString);
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
  app.UseDeveloperExceptionPage();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapHealthChecks(pattern: "/healthz");

app.MapPost(pattern: "/api/vote/{animal}",
  async Task<IResult> (
    string animal,
    IConnectionMultiplexer connection,
    IOptions<RedisOptions> redisOptions,
    ILoggerFactory loggerFactory) =>
  {
    if (!TryNormalizeAnimal(animal, out var normalizedAnimal))
      return Results.BadRequest(new { error = "Animal must be 'dog' or 'cat'." });

    var options = redisOptions.Value;
    var key = string.IsNullOrWhiteSpace(options.Key) ? "votes" : options.Key;

    var db = connection.GetDatabase();

    await db.HashIncrementAsync(key, normalizedAnimal, value: 1).ConfigureAwait(continueOnCapturedContext: false);

    var logger = loggerFactory.CreateLogger(categoryName: "VoteEndpoint");
    logger.LogInformation(message: "Recorded vote for {Animal}", normalizedAnimal);

    return Results.Accepted($"/api/votes/{normalizedAnimal}");
  });

app.MapGet(pattern: "/api/votes",
  async Task<IResult> (
    IConnectionMultiplexer connection,
    IOptions<RedisOptions> redisOptions) =>
  {
    var options = redisOptions.Value;
    var key = string.IsNullOrWhiteSpace(options.Key) ? "votes" : options.Key;

    var db = connection.GetDatabase();
    var entries = await db.HashGetAllAsync(key).ConfigureAwait(continueOnCapturedContext: false);

    var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    foreach (var entry in entries)
    {
      if (!int.TryParse(entry.Value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        value = 0;

      totals[entry.Name.ToString()] = value;
    }

    return Results.Ok(totals);
  });

app.MapFallbackToFile(filePath: "index.html");

app.Run();

static bool TryNormalizeAnimal(string value, out string normalized)
{
  normalized = string.Empty;

  if (string.IsNullOrWhiteSpace(value))
    return false;

  normalized = value.Trim().ToLowerInvariant();
  return normalized is "dog" or "cat";
}
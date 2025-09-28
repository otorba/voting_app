using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VotingApp.Shared;

namespace Worker.Services;

public sealed class RedisPollingService(
  IConnectionMultiplexer connection,
  IOptionsMonitor<RedisOptions> redisOptions,
  ILogger<RedisPollingService> logger)
  : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        var options = redisOptions.CurrentValue;
        await FetchFromRedisAsync(options).ConfigureAwait(continueOnCapturedContext: false);

        var delay = GetInterval(options);
        await Task.Delay(delay, stoppingToken).ConfigureAwait(continueOnCapturedContext: false);
      }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
      // Expected on shutdown
    }
  }

  private async Task FetchFromRedisAsync(RedisOptions options)
  {
    var redisKey = options.Key;
    if (string.IsNullOrWhiteSpace(redisKey))
    {
      logger.LogWarning(message: "Redis key is not configured; skipping fetch.");
      return;
    }

    try
    {
      var db = connection.GetDatabase();
      var entries = await db.ListRightPopAsync(redisKey, count: 50).ConfigureAwait(continueOnCapturedContext: false);
      if (entries is null)
      {
        logger.LogInformation(message: "No entries found for '{Key}'.", redisKey);
        return;
      }

      foreach (var entry in entries)
        ProcessEntry(redisKey, entry);
    }
    catch (RedisConnectionException ex)
    {
      logger.LogError(ex, message: "Failed to fetch '{Key}' due to a Redis connection error.", redisKey);
    }
    catch (RedisTimeoutException ex)
    {
      logger.LogError(ex, message: "Timed out fetching '{Key}' from Redis.", redisKey);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, message: "Unexpected error while fetching '{Key}' from Redis.", redisKey);
    }
  }

  private void ProcessEntry(string redisKey, RedisValue entry)
  {
    if (entry.IsNullOrEmpty)
    {
      logger.LogWarning(message: "Received empty payload for '{Key}'; skipping entry.", redisKey);
      return;
    }

    Vote? vote;
    try
    {
      vote = JsonSerializer.Deserialize<Vote>(entry!);
    }
    catch (JsonException ex)
    {
      logger.LogError(ex, message: "Failed to deserialize payload for '{Key}'.", redisKey);
      return;
    }

    logger.LogInformation(message: "Fetched vote for '{Animal}' at {VotedAt}.", vote?.Animal, vote?.VotedAt);
  }

  private static TimeSpan GetInterval(RedisOptions redisOptions)
  {
    var seconds = Math.Max(redisOptions.PollingIntervalSeconds, val2: 1);

    return TimeSpan.FromSeconds(seconds);
  }
}
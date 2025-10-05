using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Voting.Shared;
using Voting.Storage;

namespace Worker.Services;

public sealed class PollingService(
  IConnectionMultiplexer redisConnection,
  IOptionsMonitor<RedisOptions> redisOptions,
  IDbContextFactory<VoteContext> dbContextFactory,
  ILogger<PollingService> logger)
  : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        var options = redisOptions.CurrentValue;
        await ProcessRedisBatchAsync(options, stoppingToken).ConfigureAwait(continueOnCapturedContext: false);

        var delay = GetInterval(options);
        await Task.Delay(delay, stoppingToken).ConfigureAwait(continueOnCapturedContext: false);
      }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
      // Expected on shutdown
    }
  }

  private async Task ProcessRedisBatchAsync(RedisOptions options, CancellationToken stoppingToken)
  {
    var redisKey = options.Key;
    if (string.IsNullOrWhiteSpace(redisKey))
    {
      logger.LogWarning(message: "Redis key is not configured; skipping fetch.");
      return;
    }

    try
    {
      var db = redisConnection.GetDatabase();
      var batch = await db.ListRightPopAsync(redisKey, count: 50).ConfigureAwait(continueOnCapturedContext: false);
      if (batch is null || batch.Length == 0)
      {
        logger.LogInformation(message: "No entries found for '{Key}'.", redisKey);
        return;
      }

      await using var dbContext = await dbContextFactory.CreateDbContextAsync(stoppingToken)
        .ConfigureAwait(continueOnCapturedContext: false);

      var hasChanges = false;

      foreach (var entry in batch)
      {
        if (!TryTrackVote(redisKey, entry, dbContext))
          continue;

        hasChanges = true;
      }

      if (!hasChanges)
        return;

      try
      {
        await dbContext.SaveChangesAsync(stoppingToken).ConfigureAwait(continueOnCapturedContext: false);
      }
      catch (DbUpdateException ex)
      {
        logger.LogError(ex, message: "Failed to persist votes fetched from '{Key}'.", redisKey);
      }
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

  private bool TryTrackVote(string redisKey, RedisValue entry, VoteContext dbContext)
  {
    if (entry.IsNullOrEmpty)
    {
      logger.LogWarning(message: "Received empty payload for '{Key}'; skipping entry.", redisKey);
      return false;
    }

    Vote? vote;
    try
    {
      vote = JsonSerializer.Deserialize<Vote>(entry!);
    }
    catch (JsonException ex)
    {
      logger.LogError(ex, message: "Failed to deserialize payload for '{Key}'.", redisKey);
      return false;
    }

    if (vote is null)
    {
      logger.LogWarning(message: "Deserialized payload for '{Key}' but it produced a null vote; skipping entry.", redisKey);
      return false;
    }

    dbContext.Votes.Add(vote);
    logger.LogInformation(message: "Fetched vote for '{Animal}' at {VotedAt}.", vote.Animal, vote.VotedAt);

    return true;
  }

  private static TimeSpan GetInterval(RedisOptions redisOptions)
  {
    var seconds = Math.Max(redisOptions.PollingIntervalSeconds, val2: 1);

    return TimeSpan.FromSeconds(seconds);
  }
}
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Worker.Services;

public sealed class RedisPollingService(
  IConnectionMultiplexer connection,
  IOptionsMonitor<RedisOptions> optionsMonitor,
  ILogger<RedisPollingService> logger)
  : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        var options = optionsMonitor.CurrentValue;
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
    if (string.IsNullOrWhiteSpace(options.Key))
    {
      logger.LogWarning(message: "Redis key is not configured; skipping fetch.");
      return;
    }

    try
    {
      var db = connection.GetDatabase();
      var value = await db.StringGetAsync(options.Key).ConfigureAwait(continueOnCapturedContext: false);

      if (value.HasValue)
        logger.LogInformation(message: "Fetched '{Key}' from Redis with value: {Value}", options.Key, value.ToString());
      else
        logger.LogInformation(message: "Key '{Key}' is missing in Redis.", options.Key);
    }
    catch (RedisConnectionException ex)
    {
      logger.LogError(ex, message: "Failed to fetch '{Key}' due to a Redis connection error.", options.Key);
    }
    catch (RedisTimeoutException ex)
    {
      logger.LogError(ex, message: "Timed out fetching '{Key}' from Redis.", options.Key);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, message: "Unexpected error while fetching '{Key}' from Redis.", options.Key);
    }
  }

  private static TimeSpan GetInterval(RedisOptions options)
  {
    var seconds = Math.Max(options.PollingIntervalSeconds, val2: 1);

    return TimeSpan.FromSeconds(seconds);
  }
}
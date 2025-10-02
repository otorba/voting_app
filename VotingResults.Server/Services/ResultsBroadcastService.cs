using Microsoft.AspNetCore.SignalR;
using VotingResults.Server.Hubs;

namespace VotingResults.Server.Services;

public sealed class ResultsBroadcastService(IHubContext<ResultsHub> hubContext, ILogger<ResultsBroadcastService> logger)
  : BackgroundService
{
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(seconds: 1);

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var counter = 0;

    while (!stoppingToken.IsCancellationRequested)
    {
      counter++;

      try
      {
        await hubContext.Clients.All.SendAsync(method: "ResultsChanged", counter, stoppingToken);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, message: "Failed to broadcast results update #{Counter}", counter);
      }

      try
      {
        await Task.Delay(Interval, stoppingToken);
      }
      catch (TaskCanceledException)
      {
        // Ignore cancellation bubbling from Task.Delay when stopping.
      }
    }
  }
}
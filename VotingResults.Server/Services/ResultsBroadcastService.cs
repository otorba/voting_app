using Microsoft.AspNetCore.SignalR;
using Voting.Shared;
using VotingResults.Server.Hubs;
using VotingResults.Shared;
using VotingResults.Shared.Extensions;

namespace VotingResults.Server.Services;

public sealed class ResultsBroadcastService(
  IHubContext<ResultsHub> hubContext,
  ResultsSnapshotService snapshotService,
  ILogger<ResultsBroadcastService> logger)
  : BackgroundService
{
  private static readonly TimeSpan Interval = TimeSpan.FromSeconds(seconds: 1);
  private Dictionary<Animal, int> _previousResults = new();

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var results = await snapshotService.GetResultsAsync(stoppingToken);
      if (results.ContentEquals(_previousResults))
      {
        await WaitInterval(stoppingToken);
        continue;
      }

      _previousResults = results;

      try
      {
        await hubContext.Clients.All.SendAsync(method: "ResultsChanged",
          new VotingResultsDto { Results = results },
          stoppingToken);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, message: "Failed to broadcast results update");
      }

      await WaitInterval(stoppingToken);
    }
  }

  private static async Task WaitInterval(CancellationToken stoppingToken)
  {
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
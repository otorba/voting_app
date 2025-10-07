using Microsoft.AspNetCore.SignalR;
using VotingResults.Server.Services;
using VotingResults.Shared;

namespace VotingResults.Server.Hubs;

public sealed class ResultsHub(ResultsSnapshotService snapshotService) : Hub
{
  public Task NotifyResultsChangedAsync(VotingResultsDto votingResults) =>
    Clients.All.SendAsync(method: "ResultsChanged", votingResults);

  public async Task<VotingResultsDto> GetCurrentResults()
  {
    var results = await snapshotService.GetResultsAsync(Context.ConnectionAborted);
    return new() { Results = results };
  }
}
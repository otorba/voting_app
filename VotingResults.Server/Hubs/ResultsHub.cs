using Microsoft.AspNetCore.SignalR;

namespace VotingResults.Server.Hubs;

public class ResultsHub : Hub
{
  public Task NotifyResultsChangedAsync(int serverCounter) => Clients.All.SendAsync(method: "ResultsChanged", serverCounter);
}
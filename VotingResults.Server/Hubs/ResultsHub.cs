using Microsoft.AspNetCore.SignalR;

namespace VotingResults.Server.Hubs;

public class ResultsHub : Hub
{
  public Task NotifyResultsChangedAsync() => Clients.All.SendAsync(method: "ResultsChanged");
}
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using VotingResults.Shared;

namespace VotingResults.Client.Services;

public sealed class ResultsHubClient : IAsyncDisposable
{
  private readonly HubConnection _connection;
  private bool _started;

  public ResultsHubClient(NavigationManager navigationManager)
  {
    _connection = new HubConnectionBuilder()
      .WithUrl(navigationManager.ToAbsoluteUri(relativeUri: "/hubs/results"))
      .WithAutomaticReconnect()
      .Build();

    _connection.On<VotingResultsDto>(methodName: "ResultsChanged", NotifyResultsChangedAsync);
  }

  public async ValueTask DisposeAsync()
  {
    await _connection.StopAsync();
    await _connection.DisposeAsync();
  }

  public event Func<VotingResultsDto, Task>? ResultsChanged;

  public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
  {
    if (_started)
      return;

    if (_connection.State == HubConnectionState.Disconnected)
      await _connection.StartAsync(cancellationToken);

    _started = true;
  }

  public Task<VotingResultsDto> GetCurrentResultsAsync(CancellationToken cancellationToken = default) =>
    _connection.InvokeAsync<VotingResultsDto>(methodName: "GetCurrentResults", cancellationToken);

  private Task NotifyResultsChangedAsync(VotingResultsDto votingResults) =>
    ResultsChanged is { } handler ? handler.Invoke(votingResults) : Task.CompletedTask;
}
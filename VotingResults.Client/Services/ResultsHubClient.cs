using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

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

    _connection.On(methodName: "ResultsChanged", () => NotifyResultsChangedAsync());
  }

  public async ValueTask DisposeAsync()
  {
    await _connection.StopAsync();
    await _connection.DisposeAsync();
  }

  public event Func<Task>? ResultsChanged;

  public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
  {
    if (_started)
      return;

    if (_connection.State == HubConnectionState.Disconnected)
      await _connection.StartAsync(cancellationToken);

    _started = true;
  }

  private Task NotifyResultsChangedAsync() => ResultsChanged is { } handler ? handler.Invoke() : Task.CompletedTask;
}
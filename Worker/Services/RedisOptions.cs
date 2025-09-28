namespace Worker.Services;

public class RedisOptions
{
  public string ConnectionString { get; init; } = string.Empty;
  public int PollingIntervalSeconds { get; init; } = 2;
  public string Key { get; init; } = string.Empty;
}
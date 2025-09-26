namespace Worker.Services;

public class RedisOptions
{
  public string ConnectionString { get; set; } = string.Empty;
  public int PollingIntervalSeconds { get; set; } = 30;
  public string Key { get; set; } = string.Empty;
}
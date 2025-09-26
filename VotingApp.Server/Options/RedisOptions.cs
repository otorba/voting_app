namespace VotingApp.Server.Options;

public sealed class RedisOptions
{
  public string? ConnectionString { get; set; }
  public string? Key { get; set; }
}
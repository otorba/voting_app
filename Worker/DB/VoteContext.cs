using Microsoft.EntityFrameworkCore;
using VotingApp.Shared;

namespace Worker.DB;

public class VoteContext : DbContext
{
  public VoteContext() => Database.EnsureCreated();

  public DbSet<Vote> Votes => Set<Vote>();

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (optionsBuilder.IsConfigured)
      return;

    var connectionString = Environment.GetEnvironmentVariable(variable: "POSTGRES_CONNECTION_STRING");

    if (string.IsNullOrWhiteSpace(connectionString))
      throw new InvalidOperationException(message: "PostgreSQL connection string is not configured.");

    optionsBuilder.UseNpgsql(connectionString);
  }
}
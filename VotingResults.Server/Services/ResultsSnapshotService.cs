using Microsoft.EntityFrameworkCore;
using Voting.Shared;
using Voting.Storage;

namespace VotingResults.Server.Services;

public sealed class ResultsSnapshotService(IDbContextFactory<VoteContext> dbContextFactory)
{
  public async Task<Dictionary<Animal, int>> GetResultsAsync(CancellationToken cancellationToken = default)
  {
    await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

    return await context.Votes
      .GroupBy(v => v.Animal)
      .Select(g => new { Animal = g.Key, Count = g.Count() })
      .ToDictionaryAsync(k => k.Animal, v => v.Count, cancellationToken);
  }
}
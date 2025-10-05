using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Voting.Shared;
using VotingApp.Server.Options;
using VotingApp.Shared;

namespace VotingApp.Server.Controllers;

[ApiController]
[Route(template: "api/[controller]")]
public class VoteController(
  IOptions<RedisOptions> redisOptions,
  IConnectionMultiplexer connection,
  ILogger<VoteController> logger) : ControllerBase
{
  [HttpPost]
  public async Task<IActionResult> PostVote([FromBody] VoteRequest request)
  {
    var options = redisOptions.Value;
    var key = string.IsNullOrWhiteSpace(options.Key) ? "votes" : options.Key;

    var db = connection.GetDatabase();

    var vote = new Vote { Animal = request.Animal, VotedAt = DateTime.UtcNow };
    var serializedVote = JsonSerializer.Serialize(vote);

    await db.ListRightPushAsync(key, serializedVote).ConfigureAwait(continueOnCapturedContext: false);

    logger.LogInformation(message: "Recorded vote for '{Animal}' at {VotedAt}.", vote.Animal, vote.VotedAt);

    return Ok();
  }
}
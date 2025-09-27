using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VotingApp.Server.Options;

namespace VotingApp.Server.Controllers;

[ApiController]
[Route(template: "api/[controller]")]
public class VoteController(
  IOptions<RedisOptions> redisOptions,
  IConnectionMultiplexer connection,
  ILogger<VoteController> logger) : ControllerBase
{
  [HttpPost(template: "{animal}")]
  public async Task<IActionResult> PostVote(string animal)
  {
    if (!TryNormalizeAnimal(animal, out var normalizedAnimal))
      return BadRequest(new { error = "Animal must be 'dog' or 'cat'." });

    var options = redisOptions.Value;
    var key = string.IsNullOrWhiteSpace(options.Key) ? "votes" : options.Key;

    var db = connection.GetDatabase();

    await db.HashIncrementAsync(key, normalizedAnimal, value: 1).ConfigureAwait(continueOnCapturedContext: false);

    logger.LogInformation(message: "Recorded vote for {Animal}", normalizedAnimal);

    return Accepted($"/api/Vote/{normalizedAnimal}");
  }

  private static bool TryNormalizeAnimal(string value, out string normalized)
  {
    normalized = string.Empty;

    if (string.IsNullOrWhiteSpace(value))
      return false;

    normalized = value.Trim().ToLowerInvariant();
    return normalized is "dog" or "cat";
  }
}
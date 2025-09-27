using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
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
  public async Task<IActionResult> PostVote([FromBody] VoteRequest? request)
  {
    if (request is null)
      return BadRequest(new { error = "Request body is required." });

    if (!TryNormalizeAnimal(request.Animal, out var normalizedAnimal))
      return BadRequest(new { error = "Animal must be 'dog' or 'cat'." });

    var options = redisOptions.Value;
    var key = string.IsNullOrWhiteSpace(options.Key) ? "votes" : options.Key;

    var db = connection.GetDatabase();

    await db.HashIncrementAsync(key, normalizedAnimal, value: 1).ConfigureAwait(continueOnCapturedContext: false);

    logger.LogInformation(message: "Recorded vote for {Animal}", normalizedAnimal);

    return Accepted($"/api/Vote/{normalizedAnimal}");
  }

  private static bool TryNormalizeAnimal(Animal animal, out string normalized)
  {
    normalized = animal switch
    {
      Animal.Dog => "dog",
      Animal.Cat => "cat",
      var _ => string.Empty,
    };

    return normalized.Length > 0;
  }
}
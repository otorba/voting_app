using System.Text.Json.Serialization;
using Voting.Shared;

namespace VotingResults.Shared;

public class VotingResultsDto
{
  [JsonPropertyName(name: "r")] public Dictionary<Animal, int> Results { get; set; } = null!;
}
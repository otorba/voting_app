using System.Text.Json.Serialization;

namespace VotingApp.Shared;

public class Vote
{
  [JsonPropertyName(name: "a")] public Animal Animal { get; set; }

  [JsonPropertyName(name: "v")] public DateTime VotedAt { get; set; }
}
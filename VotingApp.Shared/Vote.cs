using System.Text.Json.Serialization;

namespace VotingApp.Shared;

public class Vote
{
  [JsonPropertyName(name: "id")] public int Id { get; set; }

  [JsonPropertyName(name: "a")] public Animal Animal { get; set; }

  [JsonPropertyName(name: "v")] public DateTime VotedAt { get; set; }
}
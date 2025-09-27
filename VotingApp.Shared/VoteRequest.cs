using System.Text.Json.Serialization;

namespace VotingApp.Shared;

public class VoteRequest
{
  [JsonPropertyName(name: "a")] public Animal Animal { get; init; }
}
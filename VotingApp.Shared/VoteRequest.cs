using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Voting.Shared;

namespace VotingApp.Shared;

public class VoteRequest : IValidatableObject
{
  [JsonPropertyName(name: "a")] public Animal Animal { get; init; }

  public IEnumerable<ValidationResult> Validate(ValidationContext context)
  {
    if (!Enum.IsDefined(Animal))
      yield return new(errorMessage: "Animal must be dog or cat.", [nameof(Animal)]);
  }
}
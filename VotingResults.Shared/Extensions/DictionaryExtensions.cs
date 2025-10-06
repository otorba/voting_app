namespace VotingResults.Shared.Extensions;

public static class DictionaryExtensions
{
  public static bool ContentEquals<TKey, TValue>(
    this IReadOnlyDictionary<TKey, TValue> source,
    IReadOnlyDictionary<TKey, TValue>? other,
    IEqualityComparer<TValue>? valueComparer = null)
    where TKey : notnull
  {
    if (ReferenceEquals(source, other))
      return true;

    if (other is null || source.Count != other.Count)
      return false;

    valueComparer ??= EqualityComparer<TValue>.Default;

    foreach (var kvp in source)
    {
      if (!other.TryGetValue(kvp.Key, out var otherValue))
        return false;

      if (!valueComparer.Equals(kvp.Value, otherValue))
        return false;
    }

    return true;
  }
}
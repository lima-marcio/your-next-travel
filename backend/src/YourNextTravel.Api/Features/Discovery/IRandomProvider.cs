namespace YourNextTravel.Api.Features.Discovery;

/// <summary>
/// Thin wrapper over Random.Shared so tests can substitute a seeded/fake source and
/// assert deterministic selection instead of depending on real randomness.
/// </summary>
public interface IRandomProvider
{
    /// <summary>Returns a random integer in [0, maxExclusive).</summary>
    int Next(int maxExclusive);
}

public class RandomProvider : IRandomProvider
{
    public int Next(int maxExclusive) => Random.Shared.Next(maxExclusive);
}

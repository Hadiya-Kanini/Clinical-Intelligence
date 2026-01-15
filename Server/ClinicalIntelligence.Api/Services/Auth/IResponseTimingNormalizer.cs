namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Abstraction for normalizing response timing to prevent timing-based enumeration attacks.
/// Implementations should ensure that response times are consistent regardless of
/// the actual processing time of the underlying operation.
/// </summary>
public interface IResponseTimingNormalizer
{
    /// <summary>
    /// Starts timing measurement. Call this at the beginning of the operation
    /// that needs timing normalization.
    /// </summary>
    /// <returns>A timing context that should be used with <see cref="NormalizeAsync"/>.</returns>
    ITimingContext StartTiming();

    /// <summary>
    /// Delays until the minimum response time has elapsed since <see cref="StartTiming"/> was called.
    /// If the elapsed time already exceeds the minimum, returns immediately.
    /// </summary>
    /// <param name="context">The timing context from <see cref="StartTiming"/>.</param>
    /// <param name="cancellationToken">Cancellation token for the delay operation.</param>
    Task NormalizeAsync(ITimingContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a timing context for tracking elapsed time during an operation.
/// </summary>
public interface ITimingContext
{
    /// <summary>
    /// Gets the elapsed time since timing started.
    /// </summary>
    TimeSpan Elapsed { get; }
}

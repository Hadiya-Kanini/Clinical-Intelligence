using System.Diagnostics;
using ClinicalIntelligence.Api.Configuration;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Implementation of <see cref="IResponseTimingNormalizer"/> that uses a stopwatch
/// to measure elapsed time and Task.Delay to pad response times to a minimum duration.
/// </summary>
public class ResponseTimingNormalizer : IResponseTimingNormalizer
{
    private readonly ForgotPasswordResponseTimingOptions _options;
    private readonly ILogger<ResponseTimingNormalizer>? _logger;
    private readonly Random _random;

    public ResponseTimingNormalizer(
        ForgotPasswordResponseTimingOptions options,
        ILogger<ResponseTimingNormalizer>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _random = new Random();
    }

    /// <inheritdoc />
    public ITimingContext StartTiming()
    {
        return new StopwatchTimingContext();
    }

    /// <inheritdoc />
    public async Task NormalizeAsync(ITimingContext context, CancellationToken cancellationToken = default)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var elapsed = context.Elapsed;
        var targetDelay = _options.MinDelayMs;

        // Add jitter if configured
        if (_options.JitterMs > 0)
        {
            targetDelay += _random.Next(0, _options.JitterMs + 1);
        }

        var remainingMs = targetDelay - (int)elapsed.TotalMilliseconds;

        if (remainingMs > 0)
        {
            _logger?.LogDebug(
                "Timing normalization: elapsed {ElapsedMs}ms, target {TargetMs}ms, delaying {DelayMs}ms",
                (int)elapsed.TotalMilliseconds,
                targetDelay,
                remainingMs);

            await Task.Delay(remainingMs, cancellationToken);
        }
        else
        {
            _logger?.LogDebug(
                "Timing normalization: elapsed {ElapsedMs}ms already exceeds target {TargetMs}ms, no delay needed",
                (int)elapsed.TotalMilliseconds,
                targetDelay);
        }
    }
}

/// <summary>
/// Timing context implementation using <see cref="Stopwatch"/>.
/// </summary>
internal sealed class StopwatchTimingContext : ITimingContext
{
    private readonly Stopwatch _stopwatch;

    public StopwatchTimingContext()
    {
        _stopwatch = Stopwatch.StartNew();
    }

    /// <inheritdoc />
    public TimeSpan Elapsed => _stopwatch.Elapsed;
}

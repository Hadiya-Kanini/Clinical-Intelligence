using System.Diagnostics;
using System.Text;
using ClinicalIntelligence.Api.Configuration;

namespace ClinicalIntelligence.Api.Services.Security;

/// <summary>
/// Malware scanner implementation using Windows Defender (MpCmdRun.exe).
/// Implements TR-018 for virus/malware scanning.
/// </summary>
public class WindowsDefenderScanner : IMalwareScanner
{
    private readonly ILogger<WindowsDefenderScanner> _logger;
    private readonly MalwareScannerOptions _options;
    private readonly bool _isAvailable;

    public string ScannerName => "WindowsDefender";

    public bool IsAvailable => _isAvailable;

    public WindowsDefenderScanner(ILogger<WindowsDefenderScanner> logger, MalwareScannerOptions options)
    {
        _logger = logger;
        _options = options;
        _isAvailable = CheckAvailability();
    }

    private bool CheckAvailability()
    {
        if (!_options.EnableScanning)
        {
            _logger.LogInformation("Malware scanning is disabled in configuration");
            return false;
        }

        if (!OperatingSystem.IsWindows())
        {
            _logger.LogWarning("Windows Defender scanner is only available on Windows");
            return false;
        }

        if (!File.Exists(_options.WindowsDefenderPath))
        {
            _logger.LogWarning("Windows Defender not found at {Path}", _options.WindowsDefenderPath);
            return false;
        }

        return true;
    }

    public async Task<MalwareScanResult> ScanAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!_isAvailable)
        {
            return MalwareScanResult.Error(ScannerName, "Scanner not available", stopwatch.Elapsed);
        }

        string? tempFilePath = null;

        try
        {
            // Save stream to temp file for scanning
            tempFilePath = Path.Combine(Path.GetTempPath(), $"scan_{Guid.NewGuid()}{Path.GetExtension(fileName)}");
            
            await using (var fileStreamOut = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Position = 0;
                await fileStream.CopyToAsync(fileStreamOut, ct);
            }

            // Run Windows Defender scan
            var result = await RunDefenderScanAsync(tempFilePath, ct);
            stopwatch.Stop();

            return result with { ScanDuration = stopwatch.Elapsed };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return MalwareScanResult.Timeout(ScannerName, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during malware scan of {FileName}", fileName);
            return MalwareScanResult.Error(ScannerName, ex.Message, stopwatch.Elapsed);
        }
        finally
        {
            // Clean up temp file
            if (tempFilePath != null && File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file {TempFile}", tempFilePath);
                }
            }
        }
    }

    private async Task<MalwareScanResult> RunDefenderScanAsync(string filePath, CancellationToken ct)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _options.WindowsDefenderPath,
            Arguments = $"-Scan -ScanType 3 -File \"{filePath}\" -DisableRemediation",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.ScanTimeoutSeconds));

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch { }

            return MalwareScanResult.Timeout(ScannerName, TimeSpan.FromSeconds(_options.ScanTimeoutSeconds));
        }

        var output = outputBuilder.ToString();
        var exitCode = process.ExitCode;

        _logger.LogDebug("Windows Defender scan completed. ExitCode={ExitCode}, Output={Output}", exitCode, output);

        // Parse exit code
        // 0 = No threats found
        // 2 = Threat found
        return exitCode switch
        {
            0 => MalwareScanResult.Clean(ScannerName, TimeSpan.Zero),
            2 => ParseThreatFromOutput(output),
            _ => MalwareScanResult.Error(ScannerName, $"Unexpected exit code: {exitCode}. Output: {output}", TimeSpan.Zero)
        };
    }

    private MalwareScanResult ParseThreatFromOutput(string output)
    {
        // Try to extract threat name from output
        // Windows Defender output format varies, but typically includes threat name
        var threatName = "Unknown Threat";
        var threatType = "Malware";

        // Look for common patterns in output
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Look for "Threat" or threat name patterns
            if (trimmedLine.Contains("Threat", StringComparison.OrdinalIgnoreCase) && trimmedLine.Contains(":"))
            {
                var parts = trimmedLine.Split(':', 2);
                if (parts.Length == 2)
                {
                    threatName = parts[1].Trim();
                }
            }

            // Categorize threat type
            if (trimmedLine.Contains("Trojan", StringComparison.OrdinalIgnoreCase))
                threatType = "Trojan";
            else if (trimmedLine.Contains("Virus", StringComparison.OrdinalIgnoreCase))
                threatType = "Virus";
            else if (trimmedLine.Contains("Worm", StringComparison.OrdinalIgnoreCase))
                threatType = "Worm";
            else if (trimmedLine.Contains("Ransomware", StringComparison.OrdinalIgnoreCase))
                threatType = "Ransomware";
            else if (trimmedLine.Contains("Spyware", StringComparison.OrdinalIgnoreCase))
                threatType = "Spyware";
        }

        _logger.LogWarning("Malware detected: ThreatName={ThreatName}, ThreatType={ThreatType}", threatName, threatType);

        return MalwareScanResult.MalwareFound(ScannerName, threatName, threatType, TimeSpan.Zero);
    }
}

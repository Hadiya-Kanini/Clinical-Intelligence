using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using ClinicalIntelligence.Api.Configuration;

namespace ClinicalIntelligence.Api.Services.Security;

/// <summary>
/// Malware scanner implementation using ClamAV daemon (clamd).
/// Provides cross-platform malware scanning via TCP protocol.
/// Implements TR-018 for virus/malware scanning.
/// </summary>
public class ClamAvScanner : IMalwareScanner
{
    private readonly ILogger<ClamAvScanner> _logger;
    private readonly MalwareScannerOptions _options;
    private readonly bool _isAvailable;

    public string ScannerName => "ClamAV";

    public bool IsAvailable => _isAvailable;

    public ClamAvScanner(ILogger<ClamAvScanner> logger, MalwareScannerOptions options)
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

        if (string.IsNullOrWhiteSpace(_options.ClamAvHost))
        {
            _logger.LogWarning("ClamAV host not configured");
            return false;
        }

        // Try to connect to ClamAV daemon
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(_options.ClamAvHost, _options.ClamAvPort);
            if (!connectTask.Wait(TimeSpan.FromSeconds(5)))
            {
                _logger.LogWarning("ClamAV daemon connection timeout at {Host}:{Port}", _options.ClamAvHost, _options.ClamAvPort);
                return false;
            }

            // Send PING command to verify daemon is responsive
            using var stream = client.GetStream();
            var pingCommand = Encoding.ASCII.GetBytes("zPING\0");
            stream.Write(pingCommand, 0, pingCommand.Length);

            var buffer = new byte[1024];
            stream.ReadTimeout = 5000;
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            var response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim('\0');

            if (response == "PONG")
            {
                _logger.LogInformation("ClamAV daemon available at {Host}:{Port}", _options.ClamAvHost, _options.ClamAvPort);
                return true;
            }

            _logger.LogWarning("Unexpected ClamAV PING response: {Response}", response);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ClamAV daemon not available at {Host}:{Port}", _options.ClamAvHost, _options.ClamAvPort);
            return false;
        }
    }

    public async Task<MalwareScanResult> ScanAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!_isAvailable)
        {
            return MalwareScanResult.Error(ScannerName, "Scanner not available", stopwatch.Elapsed);
        }

        try
        {
            using var client = new TcpClient();
            
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.ScanTimeoutSeconds));

            await client.ConnectAsync(_options.ClamAvHost!, _options.ClamAvPort, timeoutCts.Token);

            using var networkStream = client.GetStream();
            networkStream.ReadTimeout = _options.ScanTimeoutSeconds * 1000;
            networkStream.WriteTimeout = _options.ScanTimeoutSeconds * 1000;

            // Send INSTREAM command
            var instreamCommand = Encoding.ASCII.GetBytes("zINSTREAM\0");
            await networkStream.WriteAsync(instreamCommand, timeoutCts.Token);

            // Send file data in chunks
            fileStream.Position = 0;
            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, timeoutCts.Token)) > 0)
            {
                // Send chunk length (4 bytes, big-endian)
                var lengthBytes = BitConverter.GetBytes(bytesRead);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);
                
                await networkStream.WriteAsync(lengthBytes, timeoutCts.Token);
                await networkStream.WriteAsync(buffer.AsMemory(0, bytesRead), timeoutCts.Token);
            }

            // Send zero-length chunk to indicate end of stream
            var zeroLength = new byte[] { 0, 0, 0, 0 };
            await networkStream.WriteAsync(zeroLength, timeoutCts.Token);

            // Read response
            var responseBuffer = new byte[4096];
            var responseBytesRead = await networkStream.ReadAsync(responseBuffer, timeoutCts.Token);
            var response = Encoding.ASCII.GetString(responseBuffer, 0, responseBytesRead).Trim('\0').Trim();

            stopwatch.Stop();

            return ParseClamdResponse(response, stopwatch.Elapsed);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return MalwareScanResult.Timeout(ScannerName, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during ClamAV scan of {FileName}", fileName);
            return MalwareScanResult.Error(ScannerName, ex.Message, stopwatch.Elapsed);
        }
    }

    private MalwareScanResult ParseClamdResponse(string response, TimeSpan duration)
    {
        _logger.LogDebug("ClamAV response: {Response}", response);

        // Response format: "stream: OK" or "stream: <threat_name> FOUND"
        if (response.EndsWith("OK", StringComparison.OrdinalIgnoreCase))
        {
            return MalwareScanResult.Clean(ScannerName, duration);
        }

        if (response.Contains("FOUND", StringComparison.OrdinalIgnoreCase))
        {
            // Extract threat name
            // Format: "stream: Eicar-Test-Signature FOUND"
            var threatName = "Unknown Threat";
            var threatType = "Malware";

            var colonIndex = response.IndexOf(':');
            if (colonIndex >= 0)
            {
                var afterColon = response.Substring(colonIndex + 1).Trim();
                var foundIndex = afterColon.IndexOf(" FOUND", StringComparison.OrdinalIgnoreCase);
                if (foundIndex > 0)
                {
                    threatName = afterColon.Substring(0, foundIndex).Trim();
                }
            }

            // Categorize threat type based on name
            if (threatName.Contains("Trojan", StringComparison.OrdinalIgnoreCase))
                threatType = "Trojan";
            else if (threatName.Contains("Virus", StringComparison.OrdinalIgnoreCase))
                threatType = "Virus";
            else if (threatName.Contains("Worm", StringComparison.OrdinalIgnoreCase))
                threatType = "Worm";
            else if (threatName.Contains("Eicar", StringComparison.OrdinalIgnoreCase))
                threatType = "Test";

            _logger.LogWarning("ClamAV detected malware: ThreatName={ThreatName}, ThreatType={ThreatType}", threatName, threatType);

            return MalwareScanResult.MalwareFound(ScannerName, threatName, threatType, duration);
        }

        if (response.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
        {
            return MalwareScanResult.Error(ScannerName, response, duration);
        }

        // Unknown response
        _logger.LogWarning("Unknown ClamAV response: {Response}", response);
        return MalwareScanResult.Error(ScannerName, $"Unknown response: {response}", duration);
    }
}

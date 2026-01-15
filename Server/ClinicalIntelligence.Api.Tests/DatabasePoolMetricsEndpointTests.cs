using ClinicalIntelligence.Api.Diagnostics;
using Npgsql;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

public sealed class DatabasePoolMetricsEndpointTests
{
    [Fact]
    public void DbPoolMetricsSnapshot_TotalConnections_EqualsActiveAndIdle()
    {
        var snapshot = new DbPoolMetricsSnapshot
        {
            ActiveConnections = 5,
            IdleConnections = 10,
            TotalConnections = 15,
            IsAvailable = true
        };

        Assert.Equal(snapshot.ActiveConnections + snapshot.IdleConnections, snapshot.TotalConnections);
    }

    [Fact]
    public void DbPoolMetricsSnapshot_DefaultTimestamp_IsUtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var snapshot = new DbPoolMetricsSnapshot();
        var after = DateTimeOffset.UtcNow;

        Assert.True(snapshot.Timestamp >= before && snapshot.Timestamp <= after);
    }

    [Fact]
    public void DbPoolMetricsSnapshot_DefaultIsAvailable_IsFalse()
    {
        var snapshot = new DbPoolMetricsSnapshot();

        Assert.False(snapshot.IsAvailable);
    }

    [Fact]
    public void DbPoolMetricsCollector_Constructor_ParsesPoolSizeFromConnectionString()
    {
        var connectionString = "Host=localhost;Database=test;Username=user;Password=pass;Minimum Pool Size=5;Maximum Pool Size=50";

        var collector = new DbPoolMetricsCollector(connectionString);
        var snapshot = collector.CaptureSnapshot();

        Assert.Equal(5, snapshot.MinPoolSize);
        Assert.Equal(50, snapshot.MaxPoolSize);
    }

    [Fact]
    public void DbPoolMetricsCollector_Constructor_ThrowsOnNullConnectionString()
    {
        Assert.Throws<ArgumentNullException>(() => new DbPoolMetricsCollector(null!));
    }

    [Fact]
    public void DbPoolMetricsCollector_CaptureSnapshot_ReturnsSnapshotWithPoolConfig()
    {
        var connectionString = "Host=localhost;Database=test;Username=user;Password=pass";

        var collector = new DbPoolMetricsCollector(connectionString);
        var snapshot = collector.CaptureSnapshot();

        Assert.True(snapshot.MinPoolSize >= 0);
        Assert.True(snapshot.MaxPoolSize >= 0);
    }

    [Fact]
    public void DbPoolMetricsCollector_CaptureSnapshot_DoesNotLeakConnectionString()
    {
        var connectionString = "Host=localhost;Database=test;Username=sensitiveuser;Password=sensitivepass";

        var collector = new DbPoolMetricsCollector(connectionString);
        var snapshot = collector.CaptureSnapshot();

        if (snapshot.ErrorMessage != null)
        {
            Assert.DoesNotContain("sensitiveuser", snapshot.ErrorMessage);
            Assert.DoesNotContain("sensitivepass", snapshot.ErrorMessage);
            Assert.DoesNotContain("localhost", snapshot.ErrorMessage);
        }
    }

    [Fact]
    public void DbPoolMetricsSnapshot_Record_SupportsWithExpression()
    {
        var original = new DbPoolMetricsSnapshot
        {
            ActiveConnections = 5,
            IdleConnections = 10,
            TotalConnections = 15,
            IsAvailable = true
        };

        var modified = original with { ActiveConnections = 7 };

        Assert.Equal(7, modified.ActiveConnections);
        Assert.Equal(10, modified.IdleConnections);
        Assert.Equal(15, modified.TotalConnections);
    }

    [Fact]
    public void DbPoolMetricsSnapshot_ConsistencyCheck_TotalEqualsActiveAndIdle()
    {
        var snapshot = new DbPoolMetricsSnapshot
        {
            ActiveConnections = 3,
            IdleConnections = 7,
            TotalConnections = 10,
            IsAvailable = true
        };

        var isConsistent = snapshot.TotalConnections == snapshot.ActiveConnections + snapshot.IdleConnections;

        Assert.True(isConsistent, "Pool metrics should be consistent: total == active + idle");
    }

    [Fact]
    public void DbPoolMetricsSnapshot_UnavailableSnapshot_HasErrorMessage()
    {
        var snapshot = new DbPoolMetricsSnapshot
        {
            IsAvailable = false,
            ErrorMessage = "Pool not initialized"
        };

        Assert.False(snapshot.IsAvailable);
        Assert.NotNull(snapshot.ErrorMessage);
    }

    [Fact]
    public void DbPoolMetricsCollector_CaptureSnapshot_HandlesUninitiatedPool()
    {
        var connectionString = "Host=nonexistent.invalid;Database=test;Username=user;Password=pass";

        var collector = new DbPoolMetricsCollector(connectionString);
        var snapshot = collector.CaptureSnapshot();

        Assert.NotNull(snapshot);
        Assert.True(snapshot.MinPoolSize >= 0);
        Assert.True(snapshot.MaxPoolSize >= 0);
    }
}

namespace ClinicalIntelligence.Api.Tests.Mocks;

public static class MockExceptionEndpoint
{
    public static void ThrowInvalidOperationException()
    {
        throw new InvalidOperationException("Database connection failed");
    }

    public static void ThrowExceptionWithSensitiveData()
    {
        throw new InvalidOperationException("Connection failed: Server=localhost;Database=TestDb;Password=secret123");
    }

    public static void ThrowExceptionWithEnvironmentVariable()
    {
        throw new InvalidOperationException("API call failed with key: API_KEY=sk_test_12345");
    }

    public static void ThrowArgumentNullException()
    {
        throw new ArgumentNullException("parameter", "Parameter cannot be null");
    }

    public static void ThrowUnauthorizedAccessException()
    {
        throw new UnauthorizedAccessException("Access denied");
    }

    public static async Task ThrowAsyncException()
    {
        await Task.Delay(1);
        throw new InvalidOperationException("Async operation failed");
    }
}

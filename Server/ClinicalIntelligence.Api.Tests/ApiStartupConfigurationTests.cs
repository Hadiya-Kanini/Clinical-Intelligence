using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

public sealed class ApiStartupConfigurationTests
{
    [Fact]
    public void Startup_ProductionWithoutConnectionString_FailsFastWithNonSensitiveMessage()
    {
        var originalConnectionStringEnvVar = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

        try
        {
            Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", null);

            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder.UseEnvironment("Production"));

            var ex = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

            Assert.Contains("Missing required configuration value for database connection string", ex.ToString());
        }
        finally
        {
            Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", originalConnectionStringEnvVar);
        }
    }
}

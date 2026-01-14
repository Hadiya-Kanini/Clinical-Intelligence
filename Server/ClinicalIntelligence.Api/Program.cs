using ClinicalIntelligence.Api.Middleware;
using ClinicalIntelligence.Api.Results;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotNetEnv;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Load .env file from project root using DotNetEnv
var envPath = Path.Combine(builder.Environment.ContentRootPath, "..", "..", ".env");
Console.WriteLine($"Looking for .env at: {envPath}");
Console.WriteLine($"File exists: {File.Exists(envPath)}");

if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
    Console.WriteLine("Loaded .env file");
    
    // Debug: Check if environment variable is set
    var dbConn = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
    Console.WriteLine($"DATABASE_CONNECTION_STRING from env: {dbConn}");
}

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

var secrets = SecretsOptions.FromConfiguration(builder.Configuration);
var connectionString = secrets.ResolveDatabaseConnectionString(builder.Environment);

// Validate JWT configuration
secrets.ValidateJwtConfiguration();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = secrets.JwtIssuer,
            ValidAudience = secrets.JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.JwtKey!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clinical Intelligence API",
        Version = "1.0.0"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseMiddleware<RequestValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (!string.IsNullOrEmpty(path) && path.StartsWith("/api/v", StringComparison.OrdinalIgnoreCase))
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2 && string.Equals(segments[0], "api", StringComparison.OrdinalIgnoreCase) && segments[1].StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            var versionSegment = segments[1];
            if (!string.Equals(versionSegment, "v1", StringComparison.OrdinalIgnoreCase))
            {
                await ApiErrorResults.UnsupportedApiVersion(versionSegment).ExecuteAsync(context);
                return;
            }
        }
    }

    await next();
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinical Intelligence API v1");
});

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("HealthCheck")
    .WithOpenApi();

var v1 = app.MapGroup("/api/v1");

// Authentication endpoint (development only)
v1.MapPost("/auth/login", (LoginRequest request) =>
{
    // Simple authentication for development - replace with proper user management
    var email = request.Email?.Trim();
    var password = request.Password;

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        return ApiErrorResults.BadRequest(
            "invalid_input",
            "Email and password are required.",
            new[] { "email:required", "password:required" }
        );
    }

    if (password == "locked")
    {
        return ApiErrorResults.Forbidden(
            code: "account_locked",
            message: "Account temporarily locked. Please try again later."
        );
    }

    if (password == "ratelimited")
    {
        return ApiErrorResults.TooManyRequests(
            code: "rate_limited",
            message: "Too many login attempts. Please try again later."
        );
    }

    if (password != "password")
    {
        return ApiErrorResults.Unauthorized(
            code: "invalid_credentials",
            message: "Invalid email or password."
        );
    }

    var token = GenerateJwtToken(email, secrets);
    return Results.Ok(new { token = token, expires_in = secrets.JwtExpirationMinutes * 60 });
})
    .WithName("Login")
    .WithOpenApi();

v1.MapGet("/ping", () => Results.Ok(new { status = "OK" }))
    .RequireAuthorization()
    .WithName("PingV1")
    .WithOpenApi();

app.Run();

static string GenerateJwtToken(string username, SecretsOptions secrets)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.JwtKey!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new Claim("username", username)
    };

    var token = new JwtSecurityToken(
        issuer: secrets.JwtIssuer,
        audience: secrets.JwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(secrets.JwtExpirationMinutes),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

public partial class Program
{
}

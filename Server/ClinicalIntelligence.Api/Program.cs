using ClinicalIntelligence.Api.Middleware;
using ClinicalIntelligence.Api.Results;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Health;
using ClinicalIntelligence.Api.Diagnostics;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
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
var connectionString = secrets.ResolveNormalizedConnectionString(builder.Environment);

// Validate JWT configuration
secrets.ValidateJwtConfiguration();

// Determine if we're using PostgreSQL for conditional service registration
var isPostgreSql = SecretsOptions.IsPostgreSqlConnectionString(connectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseVector()));

builder.Services.AddHealthChecks()
    .AddDatabaseHealthCheck(
        connectionString: connectionString,
        latencyThresholdMs: 100,
        name: "database",
        tags: new[] { "db", "postgres" });

// Register database warm-up service and pool metrics collector for PostgreSQL only
if (isPostgreSql)
{
    builder.Services.AddHostedService(sp =>
        new DatabaseWarmupHostedService(
            connectionString,
            SecretsOptions.DefaultMinPoolSize,
            sp.GetRequiredService<ILogger<DatabaseWarmupHostedService>>()));

    builder.Services.AddSingleton(new DbPoolMetricsCollector(connectionString));
}

// Cookie name for JWT access token
const string AccessTokenCookieName = "ci_access_token";

// Add JWT Authentication with cookie support
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

        // Read JWT from HttpOnly cookie (with Authorization header fallback)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // First, try to get token from cookie
                if (context.Request.Cookies.TryGetValue(AccessTokenCookieName, out var cookieToken))
                {
                    context.Token = cookieToken;
                }
                // Fallback: Authorization header is handled automatically by the middleware
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add CORS policy for frontend with credentials support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "https://localhost:5173",
                "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

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

app.UseCors("AllowFrontend");

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

app.MapGet("/health/db", async (HttpContext context) =>
{
    var healthCheckService = context.RequestServices.GetRequiredService<HealthCheckService>();
    var report = await healthCheckService.CheckHealthAsync(
        predicate: check => check.Tags.Contains("db"),
        context.RequestAborted);

    var response = new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            latency_ms = e.Value.Data.TryGetValue("latency_ms", out var latency) ? latency : null,
            threshold_ms = e.Value.Data.TryGetValue("threshold_ms", out var threshold) ? threshold : null
        })
    };

    var statusCode = report.Status switch
    {
        HealthStatus.Healthy => StatusCodes.Status200OK,
        HealthStatus.Degraded => StatusCodes.Status200OK,
        _ => StatusCodes.Status503ServiceUnavailable
    };

    context.Response.StatusCode = statusCode;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(response);
})
    .WithName("DatabaseHealthCheck")
    .WithOpenApi();

app.MapGet("/health/db/pool", async (HttpContext context) =>
{
    var metricsCollector = context.RequestServices.GetService<DbPoolMetricsCollector>();

    if (metricsCollector == null)
    {
        return Results.Json(new
        {
            available = false,
            message = "Pool metrics not available. PostgreSQL connection pooling may not be configured."
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var snapshot = await metricsCollector.CaptureSnapshotAsync(context.RequestAborted);

    if (!snapshot.IsAvailable)
    {
        return Results.Json(new
        {
            available = false,
            message = snapshot.ErrorMessage,
            config = new
            {
                min_pool_size = snapshot.MinPoolSize,
                max_pool_size = snapshot.MaxPoolSize
            }
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    return Results.Json(new
    {
        available = true,
        active_connections = snapshot.ActiveConnections,
        idle_connections = snapshot.IdleConnections,
        total_connections = snapshot.TotalConnections,
        config = new
        {
            min_pool_size = snapshot.MinPoolSize,
            max_pool_size = snapshot.MaxPoolSize
        },
        timestamp = snapshot.Timestamp
    });
})
    .WithName("DatabasePoolMetrics")
    .WithOpenApi();

var v1 = app.MapGroup("/api/v1");

// Authentication endpoint - database-backed authentication
v1.MapPost("/auth/login", async (HttpContext context, LoginRequest request, ApplicationDbContext dbContext) =>
{
    // Validate input
    var email = request.Email?.Trim().ToLowerInvariant();
    var password = request.Password;

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        return ApiErrorResults.BadRequest(
            "invalid_input",
            "Email and password are required.",
            new[] { "email:required", "password:required" }
        );
    }

    // Query user by email (ignore query filters to check IsDeleted explicitly)
    var user = await dbContext.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Email == email);

    // Check if user exists, is not deleted, and is active
    // Use consistent error message to avoid leaking user existence
    if (user == null || user.IsDeleted || user.Status != "Active")
    {
        return ApiErrorResults.Unauthorized(
            code: "invalid_credentials",
            message: "Invalid email or password."
        );
    }

    // Check if account is locked
    if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
    {
        return ApiErrorResults.Forbidden(
            code: "account_locked",
            message: "Account temporarily locked. Please try again later."
        );
    }

    // Verify password using bcrypt
    bool passwordValid;
    try
    {
        passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }
    catch
    {
        // If hash is invalid or verification fails, treat as invalid credentials
        passwordValid = false;
    }

    if (!passwordValid)
    {
        // Increment failed login attempts
        user.FailedLoginAttempts++;
        user.UpdatedAt = DateTime.UtcNow;

        // Lock account after 5 failed attempts for 15 minutes
        if (user.FailedLoginAttempts >= 5)
        {
            user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
        }

        await dbContext.SaveChangesAsync();

        return ApiErrorResults.Unauthorized(
            code: "invalid_credentials",
            message: "Invalid email or password."
        );
    }

    // Reset failed login attempts on successful login
    user.FailedLoginAttempts = 0;
    user.LockedUntil = null;
    user.UpdatedAt = DateTime.UtcNow;
    await dbContext.SaveChangesAsync();

    // Generate JWT with role claims
    var token = GenerateJwtToken(user, secrets);

    // Set JWT in HttpOnly cookie
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = !app.Environment.IsDevelopment(), // Secure in production, allow HTTP in development
        SameSite = SameSiteMode.Lax, // Lax for same-site navigation, Strict would block cross-origin redirects
        Path = "/",
        MaxAge = TimeSpan.FromMinutes(secrets.JwtExpirationMinutes)
    };
    context.Response.Cookies.Append(AccessTokenCookieName, token, cookieOptions);

    return Results.Ok(new 
    { 
        expires_in = secrets.JwtExpirationMinutes * 60,
        user = new 
        {
            id = user.Id.ToString(),
            email = user.Email,
            role = user.Role.ToLowerInvariant()
        }
    });
})
    .WithName("Login")
    .WithOpenApi();

v1.MapPost("/auth/logout", (HttpContext context) =>
{
    // Clear the JWT cookie with matching options
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = !app.Environment.IsDevelopment(),
        SameSite = SameSiteMode.Lax,
        Path = "/"
    };
    context.Response.Cookies.Delete(AccessTokenCookieName, cookieOptions);

    return Results.Ok(new { status = "logged_out" });
})
    .WithName("Logout")
    .WithOpenApi();

v1.MapGet("/auth/me", async (HttpContext context, ApplicationDbContext dbContext) =>
{
    var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    
    if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
    {
        return ApiErrorResults.Unauthorized("invalid_token", "Invalid or missing token.");
    }

    var user = await dbContext.Users.FindAsync(userGuid);
    
    if (user == null)
    {
        return ApiErrorResults.Unauthorized("user_not_found", "User not found.");
    }

    return Results.Ok(new
    {
        id = user.Id.ToString(),
        email = user.Email,
        role = user.Role.ToLowerInvariant()
    });
})
    .RequireAuthorization()
    .WithName("GetCurrentUser")
    .WithOpenApi();

v1.MapGet("/ping", () => Results.Ok(new { status = "OK" }))
    .RequireAuthorization()
    .WithName("PingV1")
    .WithOpenApi();

app.Run();

static string GenerateJwtToken(User user, SecretsOptions secrets)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.JwtKey!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("role", user.Role),
        new Claim("name", user.Name)
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

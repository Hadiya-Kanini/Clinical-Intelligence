using ClinicalIntelligence.Api.Middleware;
using ClinicalIntelligence.Api.Results;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Contracts.Auth;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Services.Auth;
using ClinicalIntelligence.Api.Services.Email;
using ClinicalIntelligence.Api.Services.Security;
using ClinicalIntelligence.Api.Health;
using ClinicalIntelligence.Api.Diagnostics;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Validation;
using ClinicalIntelligence.Api.Authorization;
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
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
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

// Validate bcrypt configuration
secrets.ValidateBcryptConfiguration();

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
            },
            OnTokenValidated = async context =>
            {
                // Check if session has been revoked (logout enforcement)
                var sessionIdClaim = context.Principal?.FindFirst("sid")?.Value;
                
                if (string.IsNullOrEmpty(sessionIdClaim) || !Guid.TryParse(sessionIdClaim, out var sessionId))
                {
                    context.Fail("Invalid session identifier.");
                    return;
                }

                var revocationStore = context.HttpContext.RequestServices.GetRequiredService<ITokenRevocationStore>();
                var isRevoked = await revocationStore.IsSessionRevokedAsync(sessionId, context.HttpContext.RequestAborted);

                if (isRevoked)
                {
                    context.Fail("Session has been revoked.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // AdminOnly policy: requires Admin role
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(Roles.Admin));

    // Authenticated policy: requires any authenticated user
    options.AddPolicy(AuthorizationPolicies.Authenticated, policy =>
        policy.RequireAuthenticatedUser());
});

// Configure CORS policy with configuration-driven allowed origins (US_023)
var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();

// Also check environment variable directly for CORS_ALLOWED_ORIGINS
var corsOriginsFromEnv = Environment.GetEnvironmentVariable(CorsOptions.AllowedOriginsEnvVar);
if (!string.IsNullOrWhiteSpace(corsOriginsFromEnv) && string.IsNullOrWhiteSpace(corsOptions.AllowedOrigins))
{
    corsOptions.AllowedOrigins = corsOriginsFromEnv;
}

// Validate CORS configuration (fail fast in non-development if no origins configured)
corsOptions.Validate(builder.Environment.IsDevelopment());

// Determine allowed origins: use configured origins or fall back to development defaults
var allowedOrigins = corsOptions.GetParsedOrigins();
if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
{
    allowedOrigins = CorsOptions.GetDefaultDevelopmentOrigins();
    Console.WriteLine($"CORS: Using default development origins: {string.Join(", ", allowedOrigins)}");
}
else
{
    Console.WriteLine($"CORS: Configured allowed origins: {string.Join(", ", allowedOrigins)}");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsOptions.FrontendPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Register token revocation store for session-based revocation checks
builder.Services.AddScoped<ITokenRevocationStore, DbTokenRevocationStore>();

// Register bcrypt password hasher with configured work factor
builder.Services.AddSingleton<IBcryptPasswordHasher>(sp =>
    new BcryptPasswordHasher(secrets.BcryptWorkFactor, sp.GetService<ILogger<BcryptPasswordHasher>>()));

// Register email service
var smtpConfigured = secrets.ValidateSmtpConfiguration();
builder.Services.AddSingleton(secrets);
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

// Register SMTP email sender (US_026 TASK_001)
var smtpOptions = SmtpOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(smtpOptions);
builder.Services.AddSingleton<ISmtpEmailSender, SmtpEmailSender>();

// Register frontend URL options and password reset link builder (US_026 TASK_002)
var frontendUrlsOptions = FrontendUrlsOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(frontendUrlsOptions);
builder.Services.AddSingleton<IPasswordResetLinkBuilder, PasswordResetLinkBuilder>();

// Register forgot-password timing normalization options and service (US_027 TASK_001)
var forgotPasswordTimingOptions = ForgotPasswordResponseTimingOptions.FromConfiguration(builder.Configuration);
forgotPasswordTimingOptions.Validate();
builder.Services.AddSingleton(forgotPasswordTimingOptions);
builder.Services.AddSingleton<IResponseTimingNormalizer, ResponseTimingNormalizer>();

// Register password reset token service (US_025)
builder.Services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();

// Register password reset token validator (US_029 TASK_001)
builder.Services.AddScoped<IPasswordResetTokenValidator, PasswordResetTokenValidator>();

// Register password reset service (US_029 TASK_002)
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

// Register session invalidation service (US_031 TASK_001)
builder.Services.AddScoped<ISessionInvalidationService, SessionInvalidationService>();

// Register audit log writer (US_032 TASK_001)
builder.Services.AddScoped<IAuditLogWriter, AuditLogWriter>();

// Register static admin guard (US_034 TASK_002)
builder.Services.AddScoped<IStaticAdminGuard, StaticAdminGuard>();

if (smtpConfigured)
{
    Console.WriteLine("SMTP email service configured and enabled.");
}
else
{
    Console.WriteLine("SMTP email service not configured. Password reset emails will be disabled.");
}

// Configure rate limiting for login endpoint (US_015)
var rateLimitingOptions = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login rate limiting policy (US_015)
    options.AddPolicy(RateLimitingOptions.LoginPolicyName, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitingOptions.LoginPermitLimit,
                Window = rateLimitingOptions.LoginWindow,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Forgot-password rate limiting policy (US_028)
    options.AddPolicy(RateLimitingOptions.ForgotPasswordPolicyName, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitingOptions.ForgotPasswordPermitLimit,
                Window = rateLimitingOptions.ForgotPasswordWindow,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        // Determine which policy was triggered based on endpoint path
        var endpointPath = context.HttpContext.Request.Path.Value ?? string.Empty;
        var isForgotPassword = endpointPath.EndsWith("/forgot-password", StringComparison.OrdinalIgnoreCase);

        // Use appropriate window seconds based on endpoint
        var defaultWindowSeconds = isForgotPassword 
            ? rateLimitingOptions.ForgotPasswordWindowSeconds 
            : rateLimitingOptions.LoginWindowSeconds;
        var permitLimit = isForgotPassword 
            ? rateLimitingOptions.ForgotPasswordPermitLimit 
            : rateLimitingOptions.LoginPermitLimit;

        var retryAfterSeconds = defaultWindowSeconds;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            retryAfterSeconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
        }

        context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();

        // Write standardized JSON error response with endpoint-appropriate message
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var errorMessage = isForgotPassword
            ? "Too many password reset requests. Please try again later."
            : "Too many login attempts. Please try again later.";

        var errorResponse = new
        {
            error = new
            {
                code = "rate_limited",
                message = errorMessage,
                details = Array.Empty<string>()
            }
        };

        await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        // Best-effort audit logging for rate limit exceeded
        try
        {
            var dbContext = context.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();

            if (dbContext != null)
            {
                var auditEvent = new AuditLogEvent
                {
                    Id = Guid.NewGuid(),
                    UserId = null,
                    SessionId = null,
                    ActionType = "RATE_LIMIT_EXCEEDED",
                    IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.HttpContext.Request.Headers.UserAgent.ToString(),
                    ResourceType = "Auth",
                    Timestamp = DateTime.UtcNow,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        endpoint = endpointPath,
                        permitLimit = permitLimit,
                        windowSeconds = defaultWindowSeconds,
                        retryAfterSeconds = retryAfterSeconds
                    })
                };
                dbContext.AuditLogEvents.Add(auditEvent);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log to application logs if DB audit fails - do not affect 429 response
            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
            logger?.LogWarning(ex, "Failed to persist RATE_LIMIT_EXCEEDED audit event for IP {IpAddress}",
                context.HttpContext.Connection.RemoteIpAddress?.ToString());
        }
    };
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

app.UseCors(CorsOptions.FrontendPolicyName);

// Rate limiter middleware - must be early to protect endpoints
app.UseRateLimiter();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseMiddleware<RequestValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Session tracking middleware - validates server-side session and enforces inactivity timeout
app.UseMiddleware<SessionTrackingMiddleware>();

// CSRF protection middleware - validates CSRF token for state-changing requests
app.UseMiddleware<CsrfProtectionMiddleware>();

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
    .RequireAuthorization(AuthorizationPolicies.AdminOnly)
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
    .RequireAuthorization(AuthorizationPolicies.AdminOnly)
    .WithName("DatabasePoolMetrics")
    .WithOpenApi();

var v1 = app.MapGroup("/api/v1");

// Authentication endpoint - database-backed authentication
v1.MapPost("/auth/login", async (HttpContext context, LoginRequest request, ApplicationDbContext dbContext, IBcryptPasswordHasher passwordHasher) =>
{
    // Validate required fields
    var password = request.Password;

    if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrEmpty(password))
    {
        return ApiErrorResults.BadRequest(
            "invalid_input",
            "Email and password are required.",
            new[] { "email:required", "password:required" }
        );
    }

    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return ApiErrorResults.BadRequest(
            "invalid_input",
            "Email is required.",
            new[] { "email:required" }
        );
    }

    if (string.IsNullOrEmpty(password))
    {
        return ApiErrorResults.BadRequest(
            "invalid_input",
            "Password is required.",
            new[] { "password:required" }
        );
    }

    // Validate email format using RFC 5322 compliant validator (before DB query)
    var emailValidation = EmailValidation.ValidateWithDetails(request.Email);
    if (!emailValidation.IsValid)
    {
        return ApiErrorResults.BadRequest(
            "invalid_input",
            "Email format is invalid.",
            new[] { emailValidation.ErrorDetail! }
        );
    }

    var email = emailValidation.NormalizedEmail;

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
        var unlockAt = user.LockedUntil.Value;
        var remainingSeconds = (int)Math.Ceiling((unlockAt - DateTime.UtcNow).TotalSeconds);
        return ApiErrorResults.Forbidden(
            code: "account_locked",
            message: "Account temporarily locked. Please try again later.",
            details: new[] { $"unlock_at:{unlockAt:O}", $"remaining_seconds:{remainingSeconds}" }
        );
    }

    // Auto-unlock: If lockout has expired, reset counters
    if (user.LockedUntil.HasValue && user.LockedUntil.Value <= DateTime.UtcNow)
    {
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    // Verify password using centralized bcrypt hasher (timing-safe)
    var passwordValid = passwordHasher.Verify(password, user.PasswordHash);

    if (!passwordValid)
    {
        // Increment failed login attempts
        user.FailedLoginAttempts++;
        user.UpdatedAt = DateTime.UtcNow;

        // Lock account after 5 failed attempts for 30 minutes (US_016)
        var isNewlyLocked = false;
        if (user.FailedLoginAttempts >= 5 && !user.LockedUntil.HasValue)
        {
            user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
            isNewlyLocked = true;
        }

        await dbContext.SaveChangesAsync();

        // Log ACCOUNT_LOCKED audit event when lockout is triggered (US_016 TASK_003)
        if (isNewlyLocked)
        {
            try
            {
                var lockoutAuditEvent = new AuditLogEvent
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    SessionId = null,
                    ActionType = "ACCOUNT_LOCKED",
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    ResourceType = "Auth",
                    Timestamp = DateTime.UtcNow,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        unlock_at = user.LockedUntil?.ToString("O"),
                        failed_attempts = user.FailedLoginAttempts,
                        threshold = 5
                    })
                };
                dbContext.AuditLogEvents.Add(lockoutAuditEvent);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetService<ILogger<Program>>();
                logger?.LogWarning(ex, "Failed to persist ACCOUNT_LOCKED audit event for user {UserId}", user.Id);
            }
        }

        return ApiErrorResults.Unauthorized(
            code: "invalid_credentials",
            message: "Invalid email or password."
        );
    }

    // Reset failed login attempts on successful login
    user.FailedLoginAttempts = 0;
    user.LockedUntil = null;
    user.UpdatedAt = DateTime.UtcNow;

    // Use transaction to ensure atomicity of session revocation and creation
    await using var transaction = await dbContext.Database.BeginTransactionAsync();
    try
    {
        // Revoke all existing active sessions for this user (single active session enforcement)
        var activeSessions = await dbContext.Sessions
            .Where(s => s.UserId == user.Id && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        var revokedSessionIds = new List<Guid>();
        foreach (var activeSession in activeSessions)
        {
            activeSession.IsRevoked = true;
            revokedSessionIds.Add(activeSession.Id);
        }

        // Generate CSRF token for this session
        var csrfToken = CsrfProtectionMiddleware.GenerateToken();
        var csrfTokenHash = CsrfProtectionMiddleware.ComputeTokenHash(csrfToken);

        // Create server-side session record
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(secrets.JwtExpirationMinutes),
            LastActivityAt = DateTime.UtcNow,
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            IsRevoked = false,
            CsrfTokenHash = csrfTokenHash
        };
        dbContext.Sessions.Add(session);

        // Log audit event if sessions were replaced
        if (revokedSessionIds.Count > 0)
        {
            var auditEvent = new AuditLogEvent
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SessionId = session.Id,
                ActionType = "SESSION_REPLACED",
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                ResourceType = "Session",
                Timestamp = DateTime.UtcNow,
                Metadata = System.Text.Json.JsonSerializer.Serialize(new
                {
                    revokedSessionCount = revokedSessionIds.Count,
                    revokedSessionIds = revokedSessionIds
                })
            };
            dbContext.AuditLogEvents.Add(auditEvent);
        }

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        // Generate JWT with role claims and session ID
        var token = GenerateJwtToken(user, session.Id, secrets);

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
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw;
    }
})
    .RequireRateLimiting(RateLimitingOptions.LoginPolicyName)
    .WithName("Login")
    .WithOpenApi();

v1.MapPost("/auth/logout", async (HttpContext context, ITokenRevocationStore revocationStore) =>
{
    // Extract session ID from JWT claims (set by authentication middleware)
    var sessionIdClaim = context.User.FindFirst("sid")?.Value;
    
    if (!string.IsNullOrEmpty(sessionIdClaim) && Guid.TryParse(sessionIdClaim, out var sessionId))
    {
        // Revoke the session using the revocation store
        await revocationStore.RevokeSessionAsync(sessionId, context.RequestAborted);
    }

    // Clear the JWT cookie with matching options (HttpOnly, Secure, SameSite aligned to login)
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = !app.Environment.IsDevelopment(),
        SameSite = SameSiteMode.Lax,
        Path = "/",
        Expires = DateTimeOffset.UtcNow.AddDays(-1) // Explicitly expire the cookie
    };
    context.Response.Cookies.Delete(AccessTokenCookieName, cookieOptions);

    return Results.Ok(new { status = "logged_out" });
})
    .RequireAuthorization()
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

v1.MapGet("/auth/csrf", async (HttpContext context, ApplicationDbContext dbContext) =>
{
    // Extract session ID from JWT claims
    var sessionIdClaim = context.User.FindFirst("sid")?.Value;
    
    if (string.IsNullOrEmpty(sessionIdClaim) || !Guid.TryParse(sessionIdClaim, out var sessionId))
    {
        return ApiErrorResults.Unauthorized("invalid_session", "Invalid or missing session.");
    }

    // Load session to get or generate CSRF token
    var session = await dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
    
    if (session == null || session.IsRevoked)
    {
        return ApiErrorResults.Unauthorized("session_expired", "Session not found or expired.");
    }

    // Generate new CSRF token if not exists (for existing sessions without CSRF token)
    string csrfToken;
    if (string.IsNullOrEmpty(session.CsrfTokenHash))
    {
        csrfToken = CsrfProtectionMiddleware.GenerateToken();
        session.CsrfTokenHash = CsrfProtectionMiddleware.ComputeTokenHash(csrfToken);
        await dbContext.SaveChangesAsync();
    }
    else
    {
        // For security, we generate a new token that hashes to the same value
        // This is not possible with SHA-256, so we regenerate and update
        csrfToken = CsrfProtectionMiddleware.GenerateToken();
        session.CsrfTokenHash = CsrfProtectionMiddleware.ComputeTokenHash(csrfToken);
        await dbContext.SaveChangesAsync();
    }

    return Results.Ok(new CsrfTokenResponse
    {
        Token = csrfToken,
        ExpiresAt = session.ExpiresAt
    });
})
    .RequireAuthorization()
    .WithName("GetCsrfToken")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Retrieve CSRF token for state-changing requests",
        Description = "Returns a CSRF token that must be included in the X-CSRF-TOKEN header for POST, PUT, PATCH, and DELETE requests."
    });

v1.MapGet("/ping", () => Results.Ok(new { status = "OK" }))
    .RequireAuthorization()
    .WithName("PingV1")
    .WithOpenApi();

// Password Reset Flow - Forgot Password (Task 003, US_025 + US_026 + US_027 + US_032)
v1.MapPost("/auth/forgot-password", async (HttpContext context, ForgotPasswordRequest request, ApplicationDbContext dbContext, IEmailService emailService, ISmtpEmailSender smtpEmailSender, IPasswordResetTokenService tokenService, IPasswordResetLinkBuilder linkBuilder, IResponseTimingNormalizer timingNormalizer, IAuditLogWriter auditLogWriter, ILogger<Program> logger) =>
{
    // Validate email format using RFC 5322 compliant validator
    var emailValidation = EmailValidation.ValidateWithDetails(request.Email);
    if (!emailValidation.IsValid)
    {
        // Do NOT apply timing normalization to invalid input (400 responses)
        var errorMessage = emailValidation.ErrorDetail == "email:required" 
            ? "Email is required." 
            : "Email format is invalid.";
        return ApiErrorResults.BadRequest("invalid_input", errorMessage, new[] { emailValidation.ErrorDetail! });
    }

    // Start timing for syntactically valid requests (US_027 - timing normalization)
    var timingContext = timingNormalizer.StartTiming();

    var email = emailValidation.NormalizedEmail;

    // Always return success to prevent user enumeration (US_027)
    var successResponse = new { message = "If the email exists, a reset link has been sent." };

    try
    {
        // Query user by email (ignore soft-deleted users)
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.Status == "Active", context.RequestAborted);

        if (user == null)
        {
            // Log attempt without revealing user existence (US_026 - safe logging)
            logger.LogInformation("Password reset requested for non-existent or inactive account");
            // Apply timing normalization before returning (US_027)
            await timingNormalizer.NormalizeAsync(timingContext, context.RequestAborted);
            return Results.Ok(successResponse);
        }

        // Rate limiting: Check tokens created in last hour for this user
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentTokenCount = await dbContext.PasswordResetTokens
            .CountAsync(t => t.UserId == user.Id && t.ExpiresAt > oneHourAgo, context.RequestAborted);

        if (recentTokenCount >= 3)
        {
            // Apply timing normalization even for rate-limited responses (US_027)
            await timingNormalizer.NormalizeAsync(timingContext, context.RequestAborted);
            context.Response.Headers["Retry-After"] = "3600";
            return Results.Json(new
            {
                error = new
                {
                    code = "rate_limited",
                    message = "Too many password reset requests. Please try again later.",
                    details = Array.Empty<string>()
                }
            }, statusCode: StatusCodes.Status429TooManyRequests);
        }

        // Generate new token using the service (invalidates previous tokens automatically)
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id, context.RequestAborted);

        // Audit: PASSWORD_RESET_REQUESTED (US_032) - best-effort, no raw token logged
        _ = auditLogWriter.WriteAsync(
            actionType: "PASSWORD_RESET_REQUESTED",
            userId: user.Id,
            sessionId: null,
            resourceType: "Auth",
            resourceId: tokenResult.TokenId,
            ipAddress: context.Connection.RemoteIpAddress?.ToString(),
            userAgent: context.Request.Headers.UserAgent.ToString(),
            metadata: new { email = email, tokenId = tokenResult.TokenId },
            cancellationToken: context.RequestAborted);

        // Build reset URL using link builder (US_026 TASK_002)
        var resetUrl = linkBuilder.BuildResetUrl(tokenResult.PlainToken);

        // Send email via SMTP (US_026 TASK_002) - fire and forget but log outcome
        // Email sending is async and does not affect response timing
        _ = Task.Run(async () =>
        {
            try
            {
                // Use the legacy email service which has the HTML template
                var sent = await emailService.SendPasswordResetEmailAsync(user.Email, tokenResult.PlainToken, user.Name, resetUrl);
                if (sent)
                {
                    logger.LogInformation("Password reset email sent successfully for user {UserId}", user.Id);
                }
                else
                {
                    logger.LogWarning("Password reset email could not be sent for user {UserId}", user.Id);
                }
            }
            catch (Exception ex)
            {
                // Log failure without exposing token or credentials (US_026 requirement)
                logger.LogError(ex, "Failed to send password reset email for user {UserId}. Error: {ErrorType}", user.Id, ex.GetType().Name);
            }
        });

        // Apply timing normalization before returning (US_027)
        await timingNormalizer.NormalizeAsync(timingContext, context.RequestAborted);
        return Results.Ok(successResponse);
    }
    catch (OperationCanceledException)
    {
        // Request was cancelled - rethrow to let framework handle it
        throw;
    }
    catch (Exception ex)
    {
        // Log error but return generic success response to prevent enumeration (US_027)
        logger.LogError(ex, "Error processing forgot-password request. Error: {ErrorType}", ex.GetType().Name);
        // Apply timing normalization even on errors (US_027)
        await timingNormalizer.NormalizeAsync(timingContext, context.RequestAborted);
        return Results.Ok(successResponse);
    }
})
    .RequireRateLimiting(RateLimitingOptions.ForgotPasswordPolicyName)
    .WithName("ForgotPassword")
    .WithOpenApi();

// Password Reset Flow - Validate Token (US_029 TASK_001)
v1.MapGet("/auth/reset-password/validate", async (HttpContext context, IPasswordResetTokenValidator tokenValidator) =>
{
    var token = context.Request.Query["token"].ToString();

    if (string.IsNullOrWhiteSpace(token))
    {
        return ApiErrorResults.BadRequest("invalid_input", "Token is required.", new[] { "token:required" });
    }

    var result = await tokenValidator.ValidateTokenAsync(token, context.RequestAborted);

    if (!result.IsValid)
    {
        return result.InvalidReason switch
        {
            TokenInvalidReason.Missing => ApiErrorResults.BadRequest("invalid_input", "Token is required.", new[] { "token:required" }),
            TokenInvalidReason.Malformed => ApiErrorResults.BadRequest("invalid_input", "Invalid token format.", new[] { "token:invalid_format" }),
            TokenInvalidReason.NotFound => ApiErrorResults.Unauthorized("invalid_token", "Invalid or expired reset link."),
            TokenInvalidReason.Expired => ApiErrorResults.Unauthorized("token_expired", "Reset link has expired."),
            TokenInvalidReason.AlreadyUsed => ApiErrorResults.Unauthorized("token_used", "This reset link has already been used."),
            TokenInvalidReason.UserInvalid => ApiErrorResults.Unauthorized("invalid_token", "Invalid or expired reset link."),
            _ => ApiErrorResults.Unauthorized("invalid_token", "Invalid or expired reset link.")
        };
    }

    return Results.Ok(new ValidateResetPasswordTokenResponse
    {
        Valid = true,
        ExpiresAt = result.ExpiresAt
    });
})
    .WithName("ValidateResetPasswordToken")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Validate a password reset token",
        Description = "Validates whether a password reset token is valid (not expired, not used) before displaying the reset form."
    });

// Password Reset Flow - Reset Password (US_029 TASK_002 + US_032)
v1.MapPost("/auth/reset-password", async (HttpContext context, ResetPasswordRequest request, IPasswordResetService passwordResetService, IEmailService emailService, IAuditLogWriter auditLogWriter) =>
{
    var token = request.Token?.Trim();
    var newPassword = request.NewPassword;

    if (string.IsNullOrEmpty(token))
    {
        return ApiErrorResults.BadRequest("invalid_input", "Token is required.", new[] { "token:required" });
    }

    if (string.IsNullOrEmpty(newPassword))
    {
        return ApiErrorResults.BadRequest("invalid_input", "New password is required.", new[] { "newPassword:required" });
    }

    var result = await passwordResetService.ResetPasswordAsync(token, newPassword, context.RequestAborted);

    if (!result.Success)
    {
        // Audit: PASSWORD_RESET_FAILED (US_032) - best-effort, no raw token logged
        // Only log for token-related failures (not input validation failures)
        if (result.ErrorCode is "invalid_token" or "token_expired" or "token_used")
        {
            _ = auditLogWriter.WriteAsync(
                actionType: "PASSWORD_RESET_FAILED",
                userId: null,
                sessionId: null,
                resourceType: "Auth",
                resourceId: null,
                ipAddress: context.Connection.RemoteIpAddress?.ToString(),
                userAgent: context.Request.Headers.UserAgent.ToString(),
                metadata: new { reason = result.ErrorCode },
                cancellationToken: context.RequestAborted);
        }

        return result.ErrorCode switch
        {
            "invalid_input" => ApiErrorResults.BadRequest(result.ErrorCode, result.ErrorMessage!, result.ErrorDetails),
            "password_requirements_not_met" => ApiErrorResults.BadRequest(result.ErrorCode, result.ErrorMessage!, result.ErrorDetails),
            "token_expired" => ApiErrorResults.Unauthorized(result.ErrorCode, result.ErrorMessage!),
            "token_used" => ApiErrorResults.Unauthorized(result.ErrorCode, result.ErrorMessage!),
            _ => ApiErrorResults.Unauthorized(result.ErrorCode!, result.ErrorMessage!)
        };
    }

    // Audit: PASSWORD_RESET_COMPLETED (US_032) - best-effort, no raw token logged
    _ = auditLogWriter.WriteAsync(
        actionType: "PASSWORD_RESET_COMPLETED",
        userId: result.UserId,
        sessionId: null,
        resourceType: "Auth",
        resourceId: null,
        ipAddress: context.Connection.RemoteIpAddress?.ToString(),
        userAgent: context.Request.Headers.UserAgent.ToString(),
        metadata: new { userId = result.UserId },
        cancellationToken: context.RequestAborted);

    // Send confirmation email asynchronously
    _ = emailService.SendPasswordResetConfirmationAsync(result.UserEmail!, result.UserName!);

    return Results.Ok(new { message = "Password reset successful. You can now log in." });
})
    .WithName("ResetPassword")
    .WithOpenApi();

app.Run();

static string GenerateJwtToken(User user, Guid sessionId, SecretsOptions secrets)
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
        new Claim("name", user.Name),
        new Claim("sid", sessionId.ToString())
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

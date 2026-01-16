using ClinicalIntelligence.Api.Middleware;
using ClinicalIntelligence.Api.Results;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Contracts.Auth;
using ClinicalIntelligence.Api.Contracts.Admin;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Services.Auth;
using ClinicalIntelligence.Api.Services.Email;
using ClinicalIntelligence.Api.Services.Security;
using MalwareScannerOptions = ClinicalIntelligence.Api.Configuration.MalwareScannerOptions;
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

// Register document integrity validator (US_047 TASK_001)
builder.Services.AddScoped<IDocumentIntegrityValidator, DocumentIntegrityValidator>();

// Register malware scanner (US_048 TASK_001)
var malwareScannerOptions = MalwareScannerOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(malwareScannerOptions);

if (malwareScannerOptions.EnableScanning)
{
    if (malwareScannerOptions.ScannerType.Equals("ClamAV", StringComparison.OrdinalIgnoreCase) 
        && !string.IsNullOrWhiteSpace(malwareScannerOptions.ClamAvHost))
    {
        builder.Services.AddSingleton<IMalwareScanner, ClamAvScanner>();
        Console.WriteLine($"Malware scanning enabled: ClamAV at {malwareScannerOptions.ClamAvHost}:{malwareScannerOptions.ClamAvPort}");
    }
    else if (OperatingSystem.IsWindows())
    {
        builder.Services.AddSingleton<IMalwareScanner, WindowsDefenderScanner>();
        Console.WriteLine("Malware scanning enabled: Windows Defender");
    }
    else
    {
        Console.WriteLine("Malware scanning not available: No compatible scanner configured");
    }
}
else
{
    Console.WriteLine("Malware scanning disabled in configuration");
}

// Register document service (US_044 TASK_001)
builder.Services.AddScoped<IDocumentService, DocumentService>();

// Register batch upload service (US_049 TASK_001)
builder.Services.AddScoped<IBatchUploadService, BatchUploadService>();

// Register document storage service (US_050 TASK_001)
var documentStorageOptions = DocumentStorageOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(documentStorageOptions);
builder.Services.AddSingleton<IDocumentStorageService, LocalFileStorageService>();
Console.WriteLine($"Document storage configured: BasePath={documentStorageOptions.BasePath}");

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
    
    // Add cookie authentication to Swagger
    c.AddSecurityDefinition("CookieAuth", new OpenApiSecurityScheme
    {
        Name = "AccessToken",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Description = "Authentication cookie. The browser will automatically send this cookie with each request."
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CookieAuth"
                }
            },
            Array.Empty<string>()
        }
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

    // Check if user exists and is not deleted
    // Use consistent error message to avoid leaking user existence
    if (user == null || user.IsDeleted)
    {
        return ApiErrorResults.Unauthorized(
            code: "invalid_credentials",
            message: "Invalid email or password."
        );
    }

    // Check if account is locked (before password validation)
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

    // Check if account is inactive/deactivated (US_041 TASK_003)
    // Only check after password validation to provide clear message for deactivated users
    if (user.Status != "Active")
    {
        return ApiErrorResults.Forbidden(
            code: "account_inactive",
            message: "Your account has been deactivated. Please contact an administrator."
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

// Admin User Management - Create Standard User (US_037 TASK_001, US_039)
v1.MapPost("/admin/users", async (HttpContext context, CreateUserRequest request, ApplicationDbContext dbContext, IAuditLogWriter auditLogWriter, IEmailService emailService, ILogger<Program> logger) =>
{
    // Enforce admin-only access
    var isAdmin = context.User.IsInRole(Roles.Admin) || 
                  context.User.Claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == Roles.Admin);
    
    if (!isAdmin)
    {
        return ApiErrorResults.Forbidden("forbidden", "Admin access required.");
    }

    // Validate required fields
    var validationErrors = new List<string>();

    // Validate name
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        validationErrors.Add("name:required");
    }
    else if (request.Name.Length > 100)
    {
        validationErrors.Add("name:max_length_100");
    }

    // Validate email
    var emailValidation = EmailValidation.ValidateWithDetails(request.Email);
    if (!emailValidation.IsValid)
    {
        validationErrors.Add(emailValidation.ErrorDetail!);
    }

    if (validationErrors.Count > 0)
    {
        return ApiErrorResults.BadRequest("invalid_input", "Validation failed.", validationErrors);
    }

    var normalizedEmail = emailValidation.NormalizedEmail;

    // Check for duplicate email (case-insensitive, includes soft-deleted users)
    var existingUser = await dbContext.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Email == normalizedEmail, context.RequestAborted);

    if (existingUser != null)
    {
        return ApiErrorResults.Conflict("duplicate_email", "A user with this email already exists.");
    }

    // Generate server-side temporary password (US_039)
    var temporaryPassword = PasswordPolicy.GenerateSecurePassword();

    // Create user entity with hashed password
    var newUser = new User
    {
        Id = Guid.NewGuid(),
        Email = normalizedEmail,
        Name = request.Name!.Trim(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
        Role = "Standard",
        Status = "Active",
        IsStaticAdmin = false,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    try
    {
        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync(context.RequestAborted);
    }
    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("ix_users_email", StringComparison.OrdinalIgnoreCase) == true ||
                                        ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
    {
        return ApiErrorResults.Conflict("duplicate_email", "A user with this email already exists.");
    }

    // Write audit event (USER_CREATED) - do NOT log temporary password
    var adminUserId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    Guid? adminGuid = Guid.TryParse(adminUserId, out var parsed) ? parsed : null;

    _ = auditLogWriter.WriteAsync(
        actionType: "USER_CREATED",
        userId: adminGuid,
        sessionId: Guid.TryParse(context.User.FindFirst("sid")?.Value, out var sid) ? sid : null,
        resourceType: "User",
        resourceId: newUser.Id,
        ipAddress: context.Connection.RemoteIpAddress?.ToString(),
        userAgent: context.Request.Headers.UserAgent.ToString(),
        metadata: new { created_user_email = newUser.Email, created_user_role = newUser.Role },
        cancellationToken: context.RequestAborted);

    // Send credentials email (US_039) - user creation succeeds even if email fails
    var credentialsEmailSent = false;
    string? credentialsEmailErrorCode = null;

    try
    {
        if (emailService.IsConfigured)
        {
            credentialsEmailSent = await emailService.SendNewUserCredentialsEmailAsync(
                newUser.Email,
                newUser.Name,
                temporaryPassword);

            if (!credentialsEmailSent)
            {
                credentialsEmailErrorCode = "email_send_failed";
                logger.LogWarning("Credentials email could not be sent for new user {UserId}", newUser.Id);
            }
        }
        else
        {
            credentialsEmailErrorCode = "email_not_configured";
            logger.LogWarning("Email service not configured. Credentials email not sent for new user {UserId}", newUser.Id);
        }
    }
    catch (Exception ex)
    {
        credentialsEmailErrorCode = "email_send_error";
        logger.LogError(ex, "Failed to send credentials email for new user {UserId}. Error: {ErrorType}", newUser.Id, ex.GetType().Name);
    }

    return Results.Created($"/api/v1/admin/users/{newUser.Id}", new CreateUserResponse
    {
        Id = newUser.Id.ToString(),
        Email = newUser.Email,
        Role = newUser.Role.ToLowerInvariant(),
        CredentialsEmailSent = credentialsEmailSent,
        CredentialsEmailErrorCode = credentialsEmailErrorCode
    });
})
    .RequireAuthorization()
    .WithName("AdminCreateUser")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Create a new standard user (Admin only)",
        Description = "Creates a new user account with Standard role and sends credentials email. Requires admin authentication."
    });

// Admin User Management - List Users (US_040 TASK_001)
v1.MapGet("/admin/users", async (HttpContext context, ApplicationDbContext dbContext, [AsParameters] AdminUsersListQuery query) =>
{
    // Enforce admin-only access
    var isAdmin = context.User.IsInRole(Roles.Admin) || 
                  context.User.Claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == Roles.Admin);
    
    if (!isAdmin)
    {
        return ApiErrorResults.Forbidden("forbidden", "Admin access required.");
    }

    // Validate query parameters
    var validationErrors = query.Validate();
    if (validationErrors.Count > 0)
    {
        return ApiErrorResults.BadRequest("invalid_input", "Validation failed.", validationErrors);
    }

    // Normalize query parameters
    query.Normalize();

    // Build base query - exclude soft-deleted users
    var baseQuery = dbContext.Users
        .Where(u => !u.IsDeleted)
        .AsQueryable();

    // Apply search filter (case-insensitive partial match on name or email)
    if (!string.IsNullOrWhiteSpace(query.Q))
    {
        var searchTerm = query.Q.Trim().ToLowerInvariant();
        baseQuery = baseQuery.Where(u => 
            u.Name.ToLower().Contains(searchTerm) || 
            u.Email.ToLower().Contains(searchTerm));
    }

    // Get total count before pagination
    var total = await baseQuery.CountAsync(context.RequestAborted);

    // Apply sorting (whitelist-validated)
    baseQuery = query.SortBy switch
    {
        "name" => query.SortDir == "desc" 
            ? baseQuery.OrderByDescending(u => u.Name) 
            : baseQuery.OrderBy(u => u.Name),
        "email" => query.SortDir == "desc" 
            ? baseQuery.OrderByDescending(u => u.Email) 
            : baseQuery.OrderBy(u => u.Email),
        "role" => query.SortDir == "desc" 
            ? baseQuery.OrderByDescending(u => u.Role) 
            : baseQuery.OrderBy(u => u.Role),
        "status" => query.SortDir == "desc" 
            ? baseQuery.OrderByDescending(u => u.Status) 
            : baseQuery.OrderBy(u => u.Status),
        _ => baseQuery.OrderBy(u => u.Name)
    };

    // Apply pagination
    var skip = (query.Page - 1) * query.PageSize;
    var users = await baseQuery
        .Skip(skip)
        .Take(query.PageSize)
        .Select(u => new AdminUserItem
        {
            Id = u.Id.ToString(),
            Name = u.Name,
            Email = u.Email,
            Role = u.Role.ToLowerInvariant(),
            Status = u.Status.ToLowerInvariant()
        })
        .ToListAsync(context.RequestAborted);

    return Results.Ok(new AdminUsersListResponse
    {
        Items = users,
        Page = query.Page,
        PageSize = query.PageSize,
        Total = total
    });
})
    .RequireAuthorization()
    .WithName("AdminListUsers")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "List users with search, sort, and pagination (Admin only)",
        Description = "Returns a paginated list of users. Supports search by name/email, sorting by name/email/role/status, and pagination."
    });

// Admin User Management - Update User (US_041 TASK_001)
v1.MapPut("/admin/users/{userId}", async (HttpContext context, string userId, UpdateUserRequest request, ApplicationDbContext dbContext, IAuditLogWriter auditLogWriter) =>
{
    // Enforce admin-only access
    var isAdmin = context.User.IsInRole(Roles.Admin) || 
                  context.User.Claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == Roles.Admin);
    
    if (!isAdmin)
    {
        return ApiErrorResults.Forbidden("forbidden", "Admin access required.");
    }

    // Validate userId is a valid GUID
    if (!Guid.TryParse(userId, out var targetUserId))
    {
        return ApiErrorResults.BadRequest("invalid_input", "Invalid user ID format.", new[] { "userId:invalid_format" });
    }

    // Validate required fields
    var validationErrors = new List<string>();

    // Validate name
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        validationErrors.Add("name:required");
    }
    else if (request.Name.Length > 100)
    {
        validationErrors.Add("name:max_length_100");
    }

    // Validate email
    var emailValidation = EmailValidation.ValidateWithDetails(request.Email);
    if (!emailValidation.IsValid)
    {
        validationErrors.Add(emailValidation.ErrorDetail!);
    }

    // Validate role
    var allowedRoles = new[] { "admin", "standard" };
    if (string.IsNullOrWhiteSpace(request.Role))
    {
        validationErrors.Add("role:required");
    }
    else if (!allowedRoles.Contains(request.Role.ToLowerInvariant()))
    {
        validationErrors.Add("role:invalid_value");
    }

    if (validationErrors.Count > 0)
    {
        return ApiErrorResults.BadRequest("invalid_input", "Validation failed.", validationErrors);
    }

    var normalizedEmail = emailValidation.NormalizedEmail;
    var normalizedRole = request.Role!.Trim();
    normalizedRole = char.ToUpperInvariant(normalizedRole[0]) + normalizedRole.Substring(1).ToLowerInvariant();

    // Check for duplicate email (case-insensitive, includes soft-deleted users, but allow current user to keep their email)
    var existingUserWithEmail = await dbContext.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.Id != targetUserId, context.RequestAborted);

    if (existingUserWithEmail != null)
    {
        return ApiErrorResults.Conflict("duplicate_email", "A user with this email already exists.");
    }

    // Load target user (exclude soft-deleted users)
    var targetUser = await dbContext.Users
        .Where(u => !u.IsDeleted)
        .FirstOrDefaultAsync(u => u.Id == targetUserId, context.RequestAborted);

    if (targetUser == null)
    {
        return ApiErrorResults.NotFound("user_not_found", "User not found.");
    }

    // Track changed fields for audit
    var changedFields = new Dictionary<string, object>();
    if (targetUser.Name != request.Name!.Trim())
    {
        changedFields["name"] = new { from = targetUser.Name, to = request.Name!.Trim() };
    }
    if (targetUser.Email != normalizedEmail)
    {
        changedFields["email"] = new { from = targetUser.Email, to = normalizedEmail };
    }
    if (targetUser.Role != normalizedRole)
    {
        changedFields["role"] = new { from = targetUser.Role, to = normalizedRole };
    }

    // Apply update
    targetUser.Name = request.Name!.Trim();
    targetUser.Email = normalizedEmail;
    targetUser.Role = normalizedRole;
    targetUser.UpdatedAt = DateTime.UtcNow;

    await dbContext.SaveChangesAsync(context.RequestAborted);

    // Write audit event (USER_UPDATED) - best-effort
    var adminUserId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    Guid? adminGuid = Guid.TryParse(adminUserId, out var parsed) ? parsed : null;

    _ = auditLogWriter.WriteAsync(
        actionType: "USER_UPDATED",
        userId: adminGuid,
        sessionId: Guid.TryParse(context.User.FindFirst("sid")?.Value, out var sid) ? sid : null,
        resourceType: "User",
        resourceId: targetUserId,
        ipAddress: context.Connection.RemoteIpAddress?.ToString(),
        userAgent: context.Request.Headers.UserAgent.ToString(),
        metadata: new { updated_user_id = targetUserId, updated_user_email = targetUser.Email, changed_fields = changedFields },
        cancellationToken: context.RequestAborted);

    return Results.Ok(new AdminUserItem
    {
        Id = targetUser.Id.ToString(),
        Name = targetUser.Name,
        Email = targetUser.Email,
        Role = targetUser.Role.ToLowerInvariant(),
        Status = targetUser.Status.ToLowerInvariant()
    });
})
    .RequireAuthorization()
    .WithName("AdminUpdateUser")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Update user details (Admin only)",
        Description = "Updates an existing user's name, email, and role. Requires admin authentication."
    });

// Admin User Management - Toggle User Status (US_041 TASK_002)
v1.MapMethods("/admin/users/{userId}/toggle-status", new[] { "PATCH" }, async (HttpContext context, string userId, ApplicationDbContext dbContext, IStaticAdminGuard staticAdminGuard, IAuditLogWriter auditLogWriter) =>
{
    // Enforce admin-only access
    var isAdmin = context.User.IsInRole(Roles.Admin) || 
                  context.User.Claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == Roles.Admin);
    
    if (!isAdmin)
    {
        return ApiErrorResults.Forbidden("forbidden", "Admin access required.");
    }

    // Validate userId is a valid GUID
    if (!Guid.TryParse(userId, out var targetUserId))
    {
        return ApiErrorResults.BadRequest("invalid_input", "Invalid user ID format.", new[] { "userId:invalid_format" });
    }

    // Prevent self-deactivation
    var adminUserId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (Guid.TryParse(adminUserId, out var adminGuid) && adminGuid == targetUserId)
    {
        return ApiErrorResults.BadRequest("invalid_input", "You cannot change your own account status.", new[] { "userId:self_status_change" });
    }

    // Load target user (exclude soft-deleted users)
    var targetUser = await dbContext.Users
        .Where(u => !u.IsDeleted)
        .FirstOrDefaultAsync(u => u.Id == targetUserId, context.RequestAborted);

    if (targetUser == null)
    {
        return ApiErrorResults.NotFound("user_not_found", "User not found.");
    }

    // Determine new status
    var newStatus = targetUser.Status == "Active" ? "Inactive" : "Active";

    // Enforce static admin protection (FR-010c)
    try
    {
        await staticAdminGuard.ValidateCanChangeStatusAsync(targetUserId, newStatus, context.RequestAborted);
    }
    catch (StaticAdminProtectionException ex)
    {
        return ApiErrorResults.Forbidden(ex.ErrorCode, ex.Message);
    }

    var previousStatus = targetUser.Status;

    // Apply status change
    targetUser.Status = newStatus;
    targetUser.UpdatedAt = DateTime.UtcNow;

    await dbContext.SaveChangesAsync(context.RequestAborted);

    // Write audit event when transitioning to inactive (USER_DEACTIVATED)
    if (newStatus == "Inactive")
    {
        _ = auditLogWriter.WriteAsync(
            actionType: "USER_DEACTIVATED",
            userId: adminGuid,
            sessionId: Guid.TryParse(context.User.FindFirst("sid")?.Value, out var sid) ? sid : null,
            resourceType: "User",
            resourceId: targetUserId,
            ipAddress: context.Connection.RemoteIpAddress?.ToString(),
            userAgent: context.Request.Headers.UserAgent.ToString(),
            metadata: new { target_user_id = targetUserId, target_user_email = targetUser.Email, previous_status = previousStatus, new_status = newStatus },
            cancellationToken: context.RequestAborted);
    }

    return Results.Ok(new AdminUserItem
    {
        Id = targetUser.Id.ToString(),
        Name = targetUser.Name,
        Email = targetUser.Email,
        Role = targetUser.Role.ToLowerInvariant(),
        Status = targetUser.Status.ToLowerInvariant()
    });
})
    .RequireAuthorization()
    .WithName("AdminToggleUserStatus")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Toggle user status between active and inactive (Admin only)",
        Description = "Toggles a user's status. Cannot deactivate self or static admin account. Requires admin authentication."
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

// Batch Document Upload Endpoint (US_049 TASK_001)
// Enforces 10-file limit per batch (FR-014)
v1.MapPost("/documents/batch", async (HttpContext context, IBatchUploadService batchService, ILogger<Program> logger) =>
{
    var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return ApiErrorResults.Unauthorized("unauthorized", "User authentication required.");
    }

    if (!context.Request.HasFormContentType)
    {
        return ApiErrorResults.BadRequest("invalid_content_type", "Request must be multipart/form-data.");
    }

    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var files = form.Files;

    if (files.Count == 0)
    {
        return ApiErrorResults.BadRequest("missing_files", "No files provided in the request.");
    }

    var patientIdStr = form["patientId"].ToString();
    if (string.IsNullOrEmpty(patientIdStr) || !Guid.TryParse(patientIdStr, out var patientId))
    {
        return ApiErrorResults.BadRequest("invalid_patient_id", "Valid patientId is required.");
    }

    var response = await batchService.ProcessBatchAsync(files, patientId, userId, context.RequestAborted);

    if (response.BatchLimitExceeded)
    {
        logger.LogWarning("Batch upload limit exceeded: Received={Received}, Accepted={Accepted}",
            response.TotalFilesReceived, response.FilesAccepted);
    }

    return Results.Ok(response);
})
    .RequireAuthorization()
    .DisableAntiforgery()
    .WithName("BatchUploadDocuments")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Upload multiple documents in a batch",
        Description = "Uploads up to 10 documents (PDF or DOCX, max 50MB each) per batch. " +
                      "Files beyond the 10-file limit are rejected with a warning."
    });

// Document Retrieval Endpoint (US_050 TASK_001)
v1.MapGet("/documents/{documentId}/content", async (
    Guid documentId,
    HttpContext context,
    ApplicationDbContext dbContext,
    IDocumentStorageService storageService,
    ILogger<Program> logger) =>
{
    var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return ApiErrorResults.Unauthorized("unauthorized", "User authentication required.");
    }

    var document = await dbContext.Documents
        .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted, context.RequestAborted);

    if (document == null)
    {
        return ApiErrorResults.NotFound("document_not_found", "Document not found.");
    }

    var stream = await storageService.RetrieveAsync(document.StoragePath, context.RequestAborted);
    if (stream == null)
    {
        logger.LogWarning("Document file not found in storage: DocumentId={DocumentId}, Path={Path}", documentId, document.StoragePath);
        return ApiErrorResults.NotFound("file_not_found", "Document file not found in storage.");
    }

    return Results.File(stream, document.MimeType, document.OriginalName);
})
    .RequireAuthorization()
    .WithName("GetDocumentContent")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Retrieve document content",
        Description = "Downloads the original document file by document ID."
    });

// Dashboard Statistics Endpoint
v1.MapGet("/dashboard/stats", async (
    HttpContext context,
    ApplicationDbContext dbContext,
    ILogger<Program> logger) =>
{
    var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return ApiErrorResults.Unauthorized("unauthorized", "User authentication required.");
    }

    var today = DateTime.UtcNow.Date;
    var sevenDaysAgo = today.AddDays(-7);

    var uploadsToday = await dbContext.Documents
        .CountAsync(d => d.UploadedByUserId == userId && d.UploadedAt.Date == today, context.RequestAborted);

    var processing = await dbContext.Documents
        .CountAsync(d => d.UploadedByUserId == userId && d.Status == "Processing" && !d.IsDeleted, context.RequestAborted);

    var conflicts = await dbContext.Documents
        .CountAsync(d => d.UploadedByUserId == userId && d.Status == "Failed" && !d.IsDeleted, context.RequestAborted);

    var exportsLast7Days = await dbContext.Documents
        .CountAsync(d => d.UploadedByUserId == userId && d.UploadedAt >= sevenDaysAgo && d.Status == "Completed" && !d.IsDeleted, context.RequestAborted);

    var stats = new
    {
        uploadsToday,
        processing,
        conflicts,
        exportsLast7Days
    };

    return Results.Ok(stats);
})
    .RequireAuthorization()
    .WithName("GetDashboardStats")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Get dashboard statistics",
        Description = "Returns dashboard statistics including upload counts, processing status, and export counts."
    });

// Document List Endpoint
v1.MapGet("/documents", async (
    HttpContext context,
    ApplicationDbContext dbContext,
    ILogger<Program> logger) =>
{
    var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return ApiErrorResults.Unauthorized("unauthorized", "User authentication required.");
    }

    // Parse query parameters
    var page = int.TryParse(context.Request.Query["page"], out var pageValue) && pageValue > 0 ? pageValue : 1;
    var pageSize = int.TryParse(context.Request.Query["pageSize"], out var pageSizeValue) && pageSizeValue > 0 && pageSizeValue <= 100 ? pageSizeValue : 20;
    var search = context.Request.Query["search"].ToString().Trim();
    var status = context.Request.Query["status"].ToString().Trim();

    var query = dbContext.Documents
        .Where(d => d.UploadedByUserId == userId && !d.IsDeleted);

    // Apply filters
    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(d => d.OriginalName.Contains(search));
    }

    if (!string.IsNullOrEmpty(status))
    {
        query = query.Where(d => d.Status == status);
    }

    var total = await query.CountAsync(context.RequestAborted);
    var items = await query
        .OrderByDescending(d => d.UploadedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(d => new
        {
            id = d.Id.ToString(),
            fileName = d.OriginalName,
            uploadedAt = d.UploadedAt.ToString("yyyy-MM-dd HH:mm"),
            status = d.Status,
            patientId = d.PatientId.ToString(),
            fileSize = d.SizeBytes,
            errorMessage = (string)null // TODO: Add error message property if needed
        })
        .ToListAsync(context.RequestAborted);

    var result = new
    {
        items,
        total,
        page,
        pageSize
    };

    return Results.Ok(result);
})
    .RequireAuthorization()
    .WithName("ListDocuments")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "List documents",
        Description = "Returns a paginated list of documents with optional search and status filtering."
    });

// Document Upload Endpoint (US_044 TASK_001)
// Returns acknowledgment within 5 seconds for files ≤50MB (NFR-001)
v1.MapPost("/documents/upload", async (HttpContext context, IDocumentService documentService, ILogger<Program> logger) =>
{
    var startTime = DateTime.UtcNow;

    // Get user ID from JWT claims
    var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return ApiErrorResults.Unauthorized("unauthorized", "User authentication required.");
    }

    // Check for multipart form data
    if (!context.Request.HasFormContentType)
    {
        return ApiErrorResults.BadRequest("invalid_content_type", "Request must be multipart/form-data.");
    }

    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var file = form.Files.GetFile("file");

    if (file == null || file.Length == 0)
    {
        return ApiErrorResults.BadRequest("missing_file", "No file provided in the request.");
    }

    // Get patient ID from form data
    var patientIdStr = form["patientId"].ToString();
    if (string.IsNullOrEmpty(patientIdStr) || !Guid.TryParse(patientIdStr, out var patientId))
    {
        return ApiErrorResults.BadRequest("invalid_patient_id", "Valid patientId is required.");
    }

    try
    {
        var response = await documentService.ValidateAndAcknowledgeAsync(
            file,
            patientId,
            userId,
            context.RequestAborted);

        var responseTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

        logger.LogInformation(
            "Document upload endpoint completed: DocumentId={DocumentId}, IsValid={IsValid}, ResponseTimeMs={ResponseTimeMs}",
            response.DocumentId, response.IsValid, responseTimeMs);

        if (response.IsValid)
        {
            return Results.Ok(response);
        }
        else
        {
            return Results.Json(response, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Document upload failed unexpectedly");
        return ApiErrorResults.InternalServerError();
    }
})
    .RequireAuthorization()
    .DisableAntiforgery()
    .WithName("UploadDocument")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Upload a document for processing",
        Description = "Uploads a document (PDF or DOCX, max 50MB) and returns acknowledgment within 5 seconds. " +
                      "The document is validated and queued for async processing."
    });

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

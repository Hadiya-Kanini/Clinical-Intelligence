using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClinicalIntelligence.Api.Data;

/// <summary>
/// EF Core interceptor that sets PostgreSQL session variables for RLS enforcement.
/// Sets app.user_id and app.user_role from the current authenticated user's JWT claims.
/// </summary>
public sealed class NpgsqlUserContextInterceptor : DbConnectionInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NpgsqlUserContextInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetUserContext(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetUserContextAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private void SetUserContext(DbConnection connection)
    {
        var (userId, userRole) = GetUserIdentity();
        
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        using var command = connection.CreateCommand();
        command.CommandText = BuildSetContextSql(userId, userRole);
        command.ExecuteNonQuery();
    }

    private async Task SetUserContextAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        var (userId, userRole) = GetUserIdentity();
        
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = BuildSetContextSql(userId, userRole);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private (string? UserId, string? UserRole) GetUserIdentity()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return (null, null);
        }

        var userId = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var userRole = httpContext.User.FindFirst("role")?.Value
            ?? httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

        return (userId, userRole);
    }

    private static string BuildSetContextSql(string userId, string? userRole)
    {
        var safeUserId = SanitizeValue(userId);
        var safeUserRole = SanitizeValue(userRole ?? "User");
        
        return $"SET LOCAL app.user_id = '{safeUserId}'; SET LOCAL app.user_role = '{safeUserRole}';";
    }

    private static string SanitizeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        
        return value
            .Replace("'", "''")
            .Replace("\\", "\\\\")
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace("\0", "");
    }
}

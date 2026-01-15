using System;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for static admin protection (FR-010c).
/// Validates that the static admin account cannot be deleted or deactivated.
/// </summary>
public class StaticAdminProtectionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly StaticAdminGuard _guard;
    private readonly Guid _staticAdminId;
    private readonly Guid _regularUserId;

    public StaticAdminProtectionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"StaticAdminProtectionTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        var logger = Mock.Of<ILogger<StaticAdminGuard>>();
        _guard = new StaticAdminGuard(_dbContext, logger);

        // Seed test data
        _staticAdminId = Guid.NewGuid();
        _regularUserId = Guid.NewGuid();

        _dbContext.Users.AddRange(
            new User
            {
                Id = _staticAdminId,
                Email = "static-admin@example.com",
                PasswordHash = "hash",
                Name = "Static Admin",
                Role = "Admin",
                Status = "Active",
                IsStaticAdmin = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = _regularUserId,
                Email = "regular@example.com",
                PasswordHash = "hash",
                Name = "Regular User",
                Role = "Standard",
                Status = "Active",
                IsStaticAdmin = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Delete Protection Tests

    [Fact]
    public async Task ValidateCanDeleteAsync_StaticAdmin_ThrowsStaticAdminProtectionException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<StaticAdminProtectionException>(
            () => _guard.ValidateCanDeleteAsync(_staticAdminId));

        Assert.Equal("static_admin_protected", exception.ErrorCode);
        Assert.Equal("The static admin account cannot be deleted.", exception.Message);
    }

    [Fact]
    public async Task ValidateCanDeleteAsync_RegularUser_DoesNotThrow()
    {
        // Act & Assert - Should complete without exception
        await _guard.ValidateCanDeleteAsync(_regularUserId);
    }

    [Fact]
    public async Task ValidateCanDeleteAsync_NonExistentUser_DoesNotThrow()
    {
        // Act & Assert - Non-existent users are not static admins
        await _guard.ValidateCanDeleteAsync(Guid.NewGuid());
    }

    #endregion

    #region Status Change Protection Tests

    [Theory]
    [InlineData("Inactive")]
    [InlineData("Locked")]
    [InlineData("Suspended")]
    [InlineData("inactive")]
    [InlineData("INACTIVE")]
    public async Task ValidateCanChangeStatusAsync_StaticAdminToNonActive_ThrowsStaticAdminProtectionException(string newStatus)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<StaticAdminProtectionException>(
            () => _guard.ValidateCanChangeStatusAsync(_staticAdminId, newStatus));

        Assert.Equal("static_admin_protected", exception.ErrorCode);
        Assert.Contains("cannot be changed", exception.Message);
    }

    [Fact]
    public async Task ValidateCanChangeStatusAsync_StaticAdminToActive_DoesNotThrow()
    {
        // Act & Assert - Changing to Active is allowed (no-op)
        await _guard.ValidateCanChangeStatusAsync(_staticAdminId, "Active");
    }

    [Fact]
    public async Task ValidateCanChangeStatusAsync_StaticAdminToActiveCaseInsensitive_DoesNotThrow()
    {
        // Act & Assert - Case insensitive Active check
        await _guard.ValidateCanChangeStatusAsync(_staticAdminId, "ACTIVE");
        await _guard.ValidateCanChangeStatusAsync(_staticAdminId, "active");
    }

    [Theory]
    [InlineData("Inactive")]
    [InlineData("Locked")]
    [InlineData("Active")]
    public async Task ValidateCanChangeStatusAsync_RegularUser_DoesNotThrow(string newStatus)
    {
        // Act & Assert - Regular users can have any status
        await _guard.ValidateCanChangeStatusAsync(_regularUserId, newStatus);
    }

    [Fact]
    public async Task ValidateCanChangeStatusAsync_NullStatus_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _guard.ValidateCanChangeStatusAsync(_staticAdminId, null!));
    }

    [Fact]
    public async Task ValidateCanChangeStatusAsync_EmptyStatus_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _guard.ValidateCanChangeStatusAsync(_staticAdminId, ""));
    }

    [Fact]
    public async Task ValidateCanChangeStatusAsync_WhitespaceStatus_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _guard.ValidateCanChangeStatusAsync(_staticAdminId, "   "));
    }

    #endregion

    #region IsStaticAdmin Tests

    [Fact]
    public async Task IsStaticAdminAsync_StaticAdmin_ReturnsTrue()
    {
        // Act
        var result = await _guard.IsStaticAdminAsync(_staticAdminId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsStaticAdminAsync_RegularUser_ReturnsFalse()
    {
        // Act
        var result = await _guard.IsStaticAdminAsync(_regularUserId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsStaticAdminAsync_NonExistentUser_ReturnsFalse()
    {
        // Act
        var result = await _guard.IsStaticAdminAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsStaticAdminAsync_DeletedStaticAdmin_StillReturnsTrue()
    {
        // Arrange - Mark static admin as deleted (soft delete)
        var admin = await _dbContext.Users.FindAsync(_staticAdminId);
        admin!.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        // Act - Should still detect as static admin (ignores query filters)
        var result = await _guard.IsStaticAdminAsync(_staticAdminId);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Exception Factory Tests

    [Fact]
    public void StaticAdminProtectionException_CannotDelete_HasCorrectProperties()
    {
        // Act
        var exception = StaticAdminProtectionException.CannotDelete();

        // Assert
        Assert.Equal("static_admin_protected", exception.ErrorCode);
        Assert.Equal("The static admin account cannot be deleted.", exception.Message);
    }

    [Fact]
    public void StaticAdminProtectionException_CannotDeactivate_HasCorrectProperties()
    {
        // Act
        var exception = StaticAdminProtectionException.CannotDeactivate();

        // Assert
        Assert.Equal("static_admin_protected", exception.ErrorCode);
        Assert.Equal("The static admin account cannot be deactivated.", exception.Message);
    }

    [Fact]
    public void StaticAdminProtectionException_CannotChangeStatus_HasCorrectProperties()
    {
        // Act
        var exception = StaticAdminProtectionException.CannotChangeStatus("Inactive");

        // Assert
        Assert.Equal("static_admin_protected", exception.ErrorCode);
        Assert.Equal("The static admin account status cannot be changed to 'Inactive'.", exception.Message);
    }

    #endregion
}

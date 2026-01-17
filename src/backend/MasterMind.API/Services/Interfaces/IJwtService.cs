using MasterMind.API.Models.Entities;

namespace MasterMind.API.Services.Interfaces;

/// <summary>
/// JWT service interface for token generation and validation
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate JWT access token for a user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="roles">User roles</param>
    /// <returns>JWT token string</returns>
    string GenerateAccessToken(User user, IEnumerable<string> roles);

    /// <summary>
    /// Generate a refresh token
    /// </summary>
    /// <param name="ipAddress">Client IP address</param>
    /// <returns>RefreshToken entity</returns>
    RefreshToken GenerateRefreshToken(string ipAddress);

    /// <summary>
    /// Get user ID from JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID or null if invalid</returns>
    int? GetUserIdFromToken(string token);

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>True if valid</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Get token expiry time
    /// </summary>
    /// <returns>Token expiry DateTime</returns>
    DateTime GetAccessTokenExpiry();
}

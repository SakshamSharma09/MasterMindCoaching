using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.Settings;
using MasterMind.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MasterMind.API.Services.Implementations;

/// <summary>
/// JWT service implementation for token generation and validation
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly ILogger<JwtService> _logger;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtService(IOptions<JwtSettings> settings, ILogger<JwtService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("uid", user.Id.ToString()),
            new Claim("mobile", user.Mobile ?? string.Empty),
            new Claim("name", $"{user.FirstName} {user.LastName}".Trim())
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role)); // Also add as custom claim for easier access
        }

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogDebug("Generated JWT token for user {UserId}, expires at {Expiry}", user.Id, expires);
        
        return tokenString;
    }

    public RefreshToken GenerateRefreshToken(string ipAddress)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub) 
                ?? principal.FindFirst("uid");
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract user ID from token");
            return null;
        }
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            return validatedToken is JwtSecurityToken jwtToken &&
                   jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("Token has expired");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return false;
        }
    }

    public DateTime GetAccessTokenExpiry()
    {
        return DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
    }

    private TokenValidationParameters GetValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.Zero
        };
    }
}

using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LightNotes.Application.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LightNotes.Infrastructure.Identity;

internal sealed class TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    private readonly JwtOptions _jwt = options.Value;
    private readonly JsonWebTokenHandler _handler = new();

    private readonly SigningCredentials _credentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SigningKey)),
        SecurityAlgorithms.HmacSha256
    );

    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(Guid userId)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now + _jwt.AccessTokenLifetime;

        var token = _handler.CreateToken(
            new SecurityTokenDescriptor
            {
                Issuer = _jwt.Issuer,
                Audience = _jwt.Audience,
                IssuedAt = now.UtcDateTime,
                NotBefore = now.UtcDateTime,
                Expires = expiresAt.UtcDateTime,
                Subject = new ClaimsIdentity([
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                ]),
                SigningCredentials = _credentials,
            }
        );

        return (token, expiresAt);
    }

    public static (string Token, byte[] Hash) NewRefreshToken()
    {
        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));

        return (token, Hash(token));
    }

    public static byte[] Hash(string token) => SHA256.HashData(Encoding.UTF8.GetBytes(token));
}

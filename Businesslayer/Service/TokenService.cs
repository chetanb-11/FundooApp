using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Businesslayer.Interface;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ModelLayer.Model;
using ModelLayer.Model.Options;

namespace Businesslayer.Service;

public class TokenService(IOptions<JwtOptions> options, TimeProvider clock) : ITokenService
{
    private readonly JwtOptions _jwtOptions = options.Value;
    public string CreateAccessToken(UserDomainModel user)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            // new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            Expires = now.AddMinutes(_jwtOptions.AccessTokenMinutes),
            SigningCredentials = creds
            
        };
        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }

    public string? CreateResetPasswordToken(string email)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, email)
        };
        var key = new SymmetricSecurityKey((Encoding.UTF8.GetBytes(_jwtOptions.ResetSigningKey)));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            Expires = now.AddMinutes(_jwtOptions.AccessTokenMinutes),
            SigningCredentials = creds
        };
        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }
    public async Task<string?> ValidateTokenAndGetEmail(string token)
    {
        var handler = new JsonWebTokenHandler();

        var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.ResetSigningKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        });

        if (!result.IsValid)
            return null;

        var email = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        return email;
    }


    public string Hash(string token)=>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

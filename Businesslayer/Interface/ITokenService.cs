using ModelLayer.Model;

namespace Businesslayer.Interface;

public interface ITokenService
{
    string CreateAccessToken(UserDomainModel user);
    public string? CreateResetPasswordToken(string email);
    public Task<string?> ValidateTokenAndGetEmail(string token);
    string Hash(string token0);
}

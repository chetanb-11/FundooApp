using ModelLayer.Model;

namespace Businesslayer.Interface;

public interface ITokenService
{
    public string CreateAccessToken(UserDomainModel user);
    public string? CreateResetPasswordToken(string email);
    public Task<string?> ValidateTokenAndGetEmail(string token);
}

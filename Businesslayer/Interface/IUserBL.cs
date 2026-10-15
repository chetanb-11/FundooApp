using Businesslayer.Interface;
using ModelLayer.Model;

namespace Businesslayer.Interface;

public interface IUserBl
{
    Task<UserResponseDto?> RegisterUser(UserRegistrationReq user);
    public Task<bool> UserExistsAsync(string email);
    Task<LoginResponseDto?> LoginUser(UserLoginReq userReq);
    Task<bool> ResetPassword(string token, string newPassword);
    public Task<bool> ForgetPassword(string email);
}

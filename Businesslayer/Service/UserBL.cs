using Businesslayer.Interface;
using Microsoft.Extensions.Logging;
using ModelLayer.Model;
using Repository.Interface;

namespace Businesslayer.Service;

public class UserBl : IUserBl
{
    private readonly IUserRl _userRl;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserBl> _logger;

    public UserBl(IUserRl userRl, ITokenService tokenService, IEmailService emailService, ILogger<UserBl> logger)
    {
        _logger = logger;
        _emailService = emailService;
        _tokenService = tokenService;
        _userRl = userRl;
    }

    public async Task<UserResponseDto?> RegisterUser(UserRegistrationReq userModel)
    {
        _logger.LogInformation("Attempting registration for {Email}", userModel.Email);

        if (await _userRl.UserExistsByEmail(userModel.Email))
        {
            _logger.LogWarning("Registration failed: {Email} already exists", userModel.Email);
            return null;
        }

        var domainModel = new UserDomainModel
        {
            FirstName = userModel.Firstname,
            LastName = userModel.Lastname,
            Email = userModel.Email,
            Password = userModel.Password,
            ContactNumber = userModel.ContactNumber
        };

        var saved = await _userRl.RegisterUser(domainModel);
        if (saved == null)
        {
            _logger.LogError("Failed to save registered user {Email} to database", userModel.Email);
            return null;
        }

        _logger.LogInformation("User {Email} registered successfully with ID {UserId}", userModel.Email, saved.Id);
        return MapToDto(saved);
    }

    public async Task<bool> UserExistsAsync(string email) => await _userRl.UserExistsByEmail(email);

    public async Task<LoginResponseDto?> LoginUser(UserLoginReq userReq)
    {
        _logger.LogInformation("Login attempt for {Email}", userReq.Email);

        var user = await _userRl.LoginUser(userReq.Email);
        if (user == null || user.Password != userReq.Password)
        {
            _logger.LogWarning("Invalid login credentials for {Email}", userReq.Email);
            return null;
        }

        var token = _tokenService.CreateAccessToken(user);
        _logger.LogInformation("User {Email} logged in successfully", userReq.Email);

        return new LoginResponseDto
        {
            User = MapToDto(user),
            Token = token
        };
    }

    public async Task<bool> ForgetPassword(string email)
    {
        _logger.LogInformation("Forgot password requested for {Email}", email);

        var userExists = await _userRl.UserExistsByEmail(email);
        if (!userExists)
        {
            _logger.LogWarning("Forgot password requested for non-existent email {Email}", email);
            return false;
        }

        var token = _tokenService.CreateResetPasswordToken(email);
        if (token == null)
        {
            _logger.LogError("Failed to generate password reset token for {Email}", email);
            return false;
        }

        var resetUrl = "https://localhost:7004/api/fundoo/auth/reset-password?token=" + Uri.EscapeDataString(token);
        await _emailService.SendEmailAsync(email, "Reset Password", resetUrl);

        _logger.LogInformation("Password reset email sent to {Email}", email);
        return true;
    }

    public async Task<bool> ResetPassword(string token, string newPassword)
    {
        _logger.LogInformation("Attempting password reset");

        var email = await _tokenService.ValidateTokenAndGetEmail(token);
        if (email == null)
        {
            _logger.LogWarning("Password reset failed: Invalid or expired token");
            return false;
        }

        var user = await _userRl.FindExistsByEmail(email);
        if (user == null)
        {
            _logger.LogWarning("Password reset failed: No user found for token email {Email}", email);
            return false;
        }

        user.Password = newPassword;

        var result = await _userRl.SaveUser(user);
        if (result)
            _logger.LogInformation("Password reset successful for {Email}", email);
        else
            _logger.LogError("Failed to update password in database for {Email}", email);

        return result;
    }

    private static UserResponseDto MapToDto(UserDomainModel model)
    {
        return new UserResponseDto
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email
        };
    }
}

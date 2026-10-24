using BusinessLayer.Interface;
using Microsoft.Extensions.Logging;
using ModelLayer.Model;
using Repository.Interface;

namespace BusinessLayer.Service;

public class UserBl(
    IUserRl userRl,
    ITokenService tokenService,
    IEmailService emailService,
    ILogger<UserBl> logger,
    IPasswordService passwordService)
    : IUserBl
{
    public async Task<UserResponseDto?> RegisterUser(UserRegistrationReq userReq)
    {
        logger.LogInformation("Attempting registration for {Email}", userReq.Email);

        if (string.IsNullOrWhiteSpace(userReq.Password))
        {
            logger.LogWarning("Login failed: Empty password provided for {Email}", userReq.Email);
            return null;
        }
        
        if (await userRl.UserExistsByEmail(userReq.Email))
        {
            logger.LogWarning("Registration failed: {Email} already exists", userReq.Email);
            return null;
        }

        var domainModel = new UserDomainModel
        {
            FirstName = userReq.Firstname,
            LastName = userReq.Lastname,
            Email = userReq.Email,
            ContactNumber = userReq.ContactNumber
        };
        
        (domainModel.Password, domainModel.Salt) = passwordService.Hash(userReq.Password);

        var saved = await userRl.RegisterUserAsync(domainModel);
        if (saved == null)
        {
            logger.LogError("Failed to save registered user {Email} to database", userReq.Email);
            return null;
        }

        logger.LogInformation("User {Email} registered successfully with ID {UserId}", userReq.Email, saved.Id);
        return MapToDto(saved);
    }

    public async Task<bool> UserExistsAsync(string email) => await userRl.UserExistsByEmail(email);
    public async Task<bool> UserExistsByIdAsync(int userId) => await userRl.UserExistsById(userId);
    

    public async Task<LoginResponseDto?> LoginUser(UserLoginReq userReq)
    {
        logger.LogInformation("Login attempt for {Email}", userReq.Email);

        if (string.IsNullOrWhiteSpace(userReq.Password))
        {
            logger.LogWarning("Login failed: Empty password provided for {Email}", userReq.Email);
            return null;
        }
        
        var user = await userRl.LoginUserAsync(userReq.Email);
        if (user == null || user.Password == null || user.Salt == null || !passwordService.Verify(userReq.Password, user.Password, user.Salt))
        {
            logger.LogWarning("Invalid login credentials for {Email}", userReq.Email);
            return null;
        }

        var token = tokenService.CreateAccessToken(user);
        logger.LogInformation("User {Email} logged in successfully", userReq.Email);

        return new LoginResponseDto
        {
            User = MapToDto(user),
            Token = token
        };
    }

    public async Task<bool> ForgetPassword(string email)
    {
        logger.LogInformation("Forgot password requested for {Email}", email);

        var userExists = await userRl.UserExistsByEmail(email);
        if (!userExists)
        {
            logger.LogWarning("Forgot password requested for non-existent email {Email}", email);
            return false;
        }

        var token = tokenService.CreateResetPasswordToken(email);
        if (token == null)
        {
            logger.LogError("Failed to generate password reset token for {Email}", email);
            return false;
        }

        var resetUrl = "https://localhost:7004/api/fundoo/auth/reset-password?token=" + Uri.EscapeDataString(token);
        await emailService.SendEmailAsync(email, "Reset Password", resetUrl);

        logger.LogInformation("Password reset email sent to {Email}", email);
        return true;
    }

    public async Task<bool> ResetPassword(string token, string newPassword)
    {
        logger.LogInformation("Attempting password reset");
        
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            logger.LogWarning("Login failed: Empty password provided");
            return false;
        }
        var email = await tokenService.ValidateTokenAndGetEmail(token);
        if (email == null)
        {
            logger.LogWarning("Password reset failed: Invalid or expired token");
            return false;
        }

        var user = await userRl.FindExistsByEmail(email);
        if (user == null)
        {
            logger.LogWarning("Password reset failed: No user found for token email {Email}", email);
            return false;
        }

        (user.Password, user.Salt) = passwordService.Hash(newPassword);

        var result = await userRl.SaveUser(user);
        if (result)
            logger.LogInformation("Password reset successful for {Email}", email);
        else
            logger.LogError("Failed to update password in database for {Email}", email);

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

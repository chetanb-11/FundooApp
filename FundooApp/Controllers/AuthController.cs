using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using ModelLayer.Model;
using NLog.LayoutRenderers;

namespace FundooApp.Controllers;

[ApiController]
[Route("api/fundoo/[controller]")]
public class AuthController(IUserBl userBl, ILogger<AuthController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult Welcome()
    {
        return Ok("Hello from FundooController");
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? User.FindFirstValue("email");
        return Ok(new ResponseModel<string> { Success = true, Message = "User retrieved", Data = email });
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationReq req)
    {
        var result = await userBl.RegisterUser(req);
        if (result == null)
        {
            logger.LogWarning("Registration failed for {Email}", req.Email);
            return BadRequest(new ResponseModel<UserResponseDto?>
            {
                Success = false,
                Message = "Registration Failed. Email may already be registered.",
                Data = null
            });
        }
        
        logger.LogInformation("user {UserId} registered successfully", result.Id);
        return Ok(new ResponseModel<UserResponseDto>
        {
            Success = true,
            Message = "Registration Success",
            Data = result
        });
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginUser([FromBody] UserLoginReq userReq)
    {
        var result = await userBl.LoginUser(userReq);
        if (result == null)
        {
            logger.LogWarning("Failed login attempt for {Email}", userReq.Email);
            return Unauthorized(new ResponseModel<LoginResponseDto?>
            {
                Success = false,
                Message = "Invalid email or password",
                Data = null
            });
        }

        return Ok(new ResponseModel<LoginResponseDto>
        {
            Success = true,
            Message = "Login successful",
            Data = result
        });
    }

    [HttpPost]
    [Route("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        var result = await userBl.ResetPassword(req.Token, req.NewPassword);
        if (!result)
        {
            return BadRequest(new ResponseModel<string>
            {
                Success = false,
                Message = "Invalid or expired reset token",
                Data = null!
            });
        }

        return Ok(new ResponseModel<string>
        {
            Success = true,
            Message = "Password reset successfully",
            Data = "Password has been updated."
        });
    }

    [HttpPost]
    [Route("forgot-password")]
    public async Task<IActionResult> ForgetPassword(ForgotPasswordRequest req)
    {
        var result = await userBl.ForgetPassword(req.Email);
        if (!result)
        {
            logger.LogWarning("Forgot password requested for unregistered email {Email}", req.Email);
            return BadRequest(new ResponseModel<string>
            {
                Success = false,
                Message = "If your email is registered, you will receive a password reset link shortly.",
                Data = null!
            });
        }

        return Ok(new ResponseModel<string>
        {
            Success = true,
            Message = "Email sent!",
            Data = $"Reset link sent to {req.Email}"
        });
    }
}
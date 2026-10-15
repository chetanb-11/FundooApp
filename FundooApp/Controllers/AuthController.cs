using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Businesslayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using ModelLayer.Model;

namespace FundooApp.Controllers;

[ApiController]
[Route("api/fundoo/[controller]")]
public class AuthController : ControllerBase
{

    private readonly IUserBl _userBl;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserBl userBl, ILogger<AuthController> logger)
    {
        _logger = logger;
        _userBl = userBl;
    }

    [HttpGet]
    public IActionResult Welcome()
    {
        return Ok("Hello from FundooController");
    }

    [Authorize]
    [HttpGet]
    [Route("me")]
    public IActionResult Me()
    {
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? User.FindFirstValue("email");
        return Ok($"Hello from FundooApp, {email}");
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationReq req)
    {
        var result = await _userBl.RegisterUser(req);
        if (result == null)
        {
            _logger.LogWarning("Registration failed for {Email}", req.Email);
            return BadRequest(new ResponseModel<UserResponseDto?>
            {
                Success = false,
                Message = "Registration Failed. Email may already be registered.",
                Data = null
            });
        }

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
        var result = await _userBl.LoginUser(userReq);
        if (result == null)
        {
            _logger.LogWarning("Failed login attempt for {Email}", userReq.Email);
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
        var result = await _userBl.ResetPassword(req.Token, req.NewPassword);
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

    [HttpGet]
    [Route("forgot-password")]
    public async Task<IActionResult> ForgetPassword(string email)
    {
        var result = await _userBl.ForgetPassword(email);
        if (!result)
        {
            _logger.LogWarning("Forgot password requested for unregistered email {Email}", email);
            return BadRequest(new ResponseModel<string>
            {
                Success = false,
                Message = "User with entered email does not exist",
                Data = null!
            });
        }

        return Ok(new ResponseModel<string>
        {
            Success = true,
            Message = "Email sent!",
            Data = $"Reset link sent to {email}"
        });
    }
}

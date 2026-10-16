using System.Security.Claims;
using Businesslayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FundooApp.Controllers;

public class NotesController(INoteBl noteBl):ControllerBase
{
    private readonly INoteBl _noteBl = noteBl;
    
    [Authorize]
    [HttpGet]
    public IActionResult CreateNode()
    {
        // User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? User.FindFirstValue("email");
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue("Sub");
        var result = _noteBl.Create(userId);
        return Ok();
    }
    
    
    
    
}
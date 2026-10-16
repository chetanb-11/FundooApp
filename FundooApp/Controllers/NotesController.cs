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
    public async Task<IActionResult> CreateNode([FromBody]NoteRequestModel noteRequestModel)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue("Sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var result = await _noteBl.Create(noteRequestModel, userId);
        return result ? Ok() : Forbid();
    }

    public async Task<IActionResult> DeleteNode(int noteId){
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue("Sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        var result = await _noteBl.Delete(noteId, userId);
        return result ? Ok() : Forbid();
    }
}
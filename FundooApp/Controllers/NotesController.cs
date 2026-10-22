using System.Security.Claims;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.IdentityModel.JsonWebTokens;
using ModelLayer.Model;

namespace FundooApp.Controllers;

[Authorize]
[ApiController]
[Route("api/fundoo/[controller]")]
public class NotesController(INoteBl noteBl) : ControllerBase
{
    private readonly INoteBl _noteBl = noteBl;

    [HttpGet]
    // [Route("notes")]
    public async Task<IActionResult> GetAllNotesByUser()
    {
        var (isValid, userId) = TryGetUserId();
        if (!isValid)
            return Unauthorized(new ResponseModel<string>() { Success = false, Message = "Invalid user token" });
        var notesByUser = await _noteBl.GetAllNotesByUser(userId);
        return Ok(new ResponseModel<IEnumerable<NoteResponseDto>>()
        {
            Success = true,
            Message = "notes fetched successfully",
            Data = notesByUser,
        });
    }
    
    [Route("{noteId:int}")]
    [HttpGet]
    public async Task<IActionResult> GetNoteById(int noteId)
    {
        var (isValid, userId) = TryGetUserId();
        if (!isValid)
            return Unauthorized(new ResponseModel<string>() { Success = false, Message = "Invalid user token" });
        if (noteId == null)
        {
            return NotFound(new ResponseModel<string>
            {
                Success = false,
                Message = "Note not found"
            });
        }
        var noteById = await _noteBl.GetNoteById(noteId, userId);
        
        return Ok(new ResponseModel<NoteResponseDto>()
        {
            Success = true,
            Message = "notes fetched successfully",
            Data = noteById,
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] NoteRequestModel noteRequestModel)
    {
        var (isValid, userId) = TryGetUserId();
        if (!isValid)
            return Unauthorized(new ResponseModel<string> { Success = false, Message = "Invalid user token" });

        var createdNote = await _noteBl.CreateAsync(noteRequestModel, userId);
        if (createdNote == null)
        {
            return BadRequest(new ResponseModel<string> { Success = false, Message = "Failed to create note" });
        }

        return Ok(new ResponseModel<NoteResponseDto>
        {
            Success = true,
            Message = "Note created successfully",
            Data = createdNote
        });
    }

    [HttpPut("{noteId:int}")]
    public async Task<IActionResult> UpdateNote([FromRoute] int noteId, [FromBody] NoteRequestModel note)
    {
        var (isValid, userId) = TryGetUserId();
        if (!isValid)
            return Unauthorized(new ResponseModel<string> { Success = false, Message = "Invalid user token" });

        var updated = await _noteBl.UpdateAsync(new NoteDomainModel(){
            Id = noteId,
            Title = note.Title, 
            Description = note.Description,
            UserId = userId
        });
        if (!updated)
        {
            return NotFound(new ResponseModel<string> { Success = false, Message = "Note not found or update failed" });
        }

        return Ok(new ResponseModel<NoteResponseDto>
        {
            Success = true,
            Message = "Note updated successfully",
            Data = new NoteResponseDto { Id = noteId, Title = note.Title, Description = note.Description }
        });
    }

    [HttpDelete("{noteId:int}")]
    public async Task<IActionResult> DeleteNote([FromRoute] int noteId)
    {
        var (isValid, userId) = TryGetUserId();
        if (!isValid)
            return Unauthorized(new ResponseModel<string> { Success = false, Message = "Invalid user token" });

        var result = await _noteBl.DeleteAsync(noteId, userId);
        if (!result)
        {
            return NotFound(new ResponseModel<string>
                { Success = false, Message = "Note not found or could not be deleted" });
        }

        return Ok(new ResponseModel<string> { Success = true, Message = "Note deleted successfully" });
    }

    [HttpPost]
    [Route("{noteId:int}/archive")]
    public async Task<IActionResult> ArchiveNote(int noteId)
    {
        var (isValid, userId) = TryGetUserId();
        if (!isValid)
            return Unauthorized(new ResponseModel<string> { Success = false, Message = "Invalid user token" });

        var note = await _noteBl.ArchiveNote(noteId, userId);
        if (note == null)
        {
            return NotFound(new ResponseModel<string>()
            {
                Success = false, Message = "Note not found or could not be Archived"
            });
        }

        return Ok(new ResponseModel<NoteResponseDto>()
        {
            Success = true,
            Message = "Note Archived",
            Data = note,
        });
    }
    [HttpPost]
    [Route("{noteId:int}/unarchive")]
    
    public async Task<IActionResult> UnArchiveNote(int noteId)
    {
        var (isValid, userId) = TryGetUserId();
        if (!isValid)
            return Unauthorized(new ResponseModel<string> { Success = false, Message = "Invalid user token" });

        var note = await _noteBl.UnArchiveNote(noteId, userId);
        if (note == null)
        {
            return NotFound(new ResponseModel<string>()
            {
                Success = false, Message = "Note not found or could not be UnArchived"
            });
        }

        return Ok(new ResponseModel<NoteResponseDto>()
        {
            Success = true,
            Message = "Note UnArchived",
            Data = note,
        });
    }
    [HttpGet]
    [Route("archived")]
    public async Task<IActionResult> GetArchivedNotes()
    {
        var (isValid, userId) = TryGetUserId();
        if (!isValid)
            return Unauthorized(new ResponseModel<string> { Success = false, Message = "Invalid user token" });
        
        var notes = await _noteBl.GetArchivedNotes(userId);

        return Ok(new ResponseModel<List<NoteResponseDto>>
        {
            Success = true,
            Message = "Archived notes retrieved successfully.",
            Data = notes
        });
    }
    
    private (bool IsValid, int UserId) TryGetUserId()
    {
        var claimValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                         User.FindFirstValue("Sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claimValue, out var userId) && userId > 0)
        {
            return (true, userId);
        }

        return (false, 0);
    }
    
    
}
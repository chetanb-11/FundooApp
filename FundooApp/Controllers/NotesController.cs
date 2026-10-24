using System.Security.Claims;
using BusinessLayer.Interface;
using FundooApp.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using ModelLayer.Model;

namespace FundooApp.Controllers;

[Authorize]
[ApiController]
[ServiceFilter(typeof(ValidateUserContextFilter))]
[Route("api/fundoo/[controller]")]
public class NotesController(INoteBl noteBl, ILogger<NotesController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieve all active notes by user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllNotesByUser()
    {
        var userId = (int)HttpContext.Items["userId"]!;
        var notesByUser = await noteBl.GetAllNotesByUser(userId);

        logger.LogInformation("Retrieved notes for user {UserId}", userId);
        return Ok(new ResponseModel<IEnumerable<NoteResponseDto>>()
        {
            Success = true,
            Message = "notes fetched successfully",
            Data = notesByUser,
        });
    }

    /// <summary>
    /// Get note by id
    /// </summary>
    /// <param name="noteId"></param>
    /// <returns></returns>
    [Route("{noteId:int}")]
    [HttpGet]
    public async Task<IActionResult> GetNoteById(int noteId)
    {
        var userId = (int)HttpContext.Items["userId"]!;
        
        var noteById = await noteBl.GetNoteById(noteId, userId);

        if (noteById == null)
        {
            logger.LogWarning("note not found for note: {NoteId} for user: {UserId}", noteId, userId);
            return NotFound(new ResponseModel<string>
            {
                Success = false,
                Message = "Note not found"
            });
        }

        logger.LogInformation("Retrieved note: {NoteId} for user: {UserId}", noteId, userId);
        return Ok(new ResponseModel<NoteResponseDto>()
        {
            Success = true,
            Message = "notes fetched successfully",
            Data = noteById,
        });
    }

    /// <summary>
    /// Create a new note for logged-in user
    /// </summary>
    /// <param name="noteRequestModel"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] NoteRequestModel noteRequestModel)
    {
        var userId = (int)HttpContext.Items["userId"]!;
        
        logger.LogInformation("creating note with title {Title}", noteRequestModel.Title);
        var createdNote = await noteBl.CreateAsync(noteRequestModel, userId);
        if (createdNote == null)
        {
            logger.LogWarning("Failed to create note {Title}", noteRequestModel.Title);
            return BadRequest(new ResponseModel<string> { Success = false, Message = "Failed to create note" });
        }

        logger.LogInformation("note {NoteId} created", createdNote.Id);
        return Ok(new ResponseModel<NoteResponseDto>
        {
            Success = true,
            Message = "Note created successfully",
            Data = createdNote
        });
    }

    /// <summary>
    /// Update the note by noteId
    /// </summary>
    /// <param name="noteId"></param>
    /// <param name="note"></param>
    /// <returns></returns>
    [HttpPut("{noteId:int}")]
    public async Task<IActionResult> UpdateNote([FromRoute] int noteId, [FromBody] NoteRequestModel note)
    {
        var userId = (int)HttpContext.Items["userId"]!;
        
        logger.LogInformation("updating note with title {Title}", note.Title);

        var updated = await noteBl.UpdateAsync(new NoteDomainModel()
        {
            Id = noteId,
            Title = note.Title,
            Description = note.Description,
            UserId = userId
        });
        if (!updated)
        {
            logger.LogWarning("Note {NoteId} not found or update failed for user: {UserId}", noteId, userId);
            return NotFound(new ResponseModel<string> { Success = false, Message = "Note not found or update failed" });
        }

        logger.LogInformation("Note {NoteId} updated for user: {UserId}", noteId, userId);
        return Ok(new ResponseModel<NoteResponseDto>
        {
            Success = true,
            Message = "Note updated successfully",
            Data = new NoteResponseDto { Id = noteId, Title = note.Title, Description = note.Description }
        });
    }

    /// <summary>
    /// Delete note with given noteId
    /// </summary>
    /// <param name="noteId"></param>
    /// <returns></returns>
    [HttpDelete("{noteId:int}")]
    public async Task<IActionResult> DeleteNote([FromRoute] int noteId)
    {
        var userId = (int)HttpContext.Items["userId"]!;
        
        logger.LogInformation("deleting note {NoteId}", noteId);
        var result = await noteBl.DeleteAsync(noteId, userId);
        if (!result)
        {
            logger.LogWarning("Deleting failed, note {NoteId} could not be deleted", noteId);
            return NotFound(new ResponseModel<string>
                { Success = false, Message = "Note not found or could not be deleted" });
        }

        logger.LogInformation("Deleted note {NoteId}", noteId);
        return Ok(new ResponseModel<string> { Success = true, Message = "Note deleted successfully" });
    }

    /// <summary>
    /// Archive note with given NoteId
    /// </summary>
    /// <param name="noteId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{noteId:int}/archive")]
    public async Task<IActionResult> ArchiveNote(int noteId)
    {
        logger.LogInformation("Archiving note {NoteId}", noteId);
        var userId = (int)HttpContext.Items["userId"]!;
        
        var note = await noteBl.ArchiveNote(noteId, userId);
        if (note == null)
        {
            logger.LogWarning("Archiving failed, note {NoteId} not Archived", noteId);
            return NotFound(new ResponseModel<string>()
            {
                Success = false, Message = "Note not found or could not be Archived"
            });
        }

        logger.LogInformation("Archived note {NoteId}", noteId);
        return Ok(new ResponseModel<NoteResponseDto>()
        {
            Success = true,
            Message = "Note Archived",
            Data = note,
        });
    }

    /// <summary>
    /// Unarchive note with given noteId
    /// </summary>
    /// <param name="noteId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{noteId:int}/unarchive")]
    public async Task<IActionResult> UnArchiveNote(int noteId)
    {
        logger.LogInformation("Unarchiving note {NoteId}", noteId);
        var userId = (int)HttpContext.Items["userId"]!;
        
        var note = await noteBl.UnArchiveNote(noteId, userId);
        if (note == null)
        {
            logger.LogWarning("Not unarchived note {NoteId}", noteId);
            return NotFound(new ResponseModel<string>()
            {
                Success = false, Message = "Note not found or could not be UnArchived"
            });
        }

        logger.LogInformation("unarchived note {NoteId}", noteId);
        return Ok(new ResponseModel<NoteResponseDto>()
        {
            Success = true,
            Message = "Note UnArchived",
            Data = note,
        });
    }

    /// <summary>
    /// Retrieve Archived notes
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("archived")]
    public async Task<IActionResult> GetArchivedNotes()
    {
        var userId = (int)HttpContext.Items["userId"]!;

        var notes = await noteBl.GetArchivedNotes(userId);

        logger.LogInformation("Retrieved Archived notes");
        return Ok(new ResponseModel<List<NoteResponseDto>>
        {
            Success = true,
            Message = "Archived notes retrieved successfully.",
            Data = notes
        });
    }

    /// <summary>
    /// Trash note with given NoteId
    /// </summary>
    /// <param name="noteId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{noteId:int}/trash")]
    public async Task<IActionResult> TrashNote(int noteId)
    {
        logger.LogInformation("Trashing note {NoteId}", noteId);
        var userId = (int)HttpContext.Items["userId"]!;

        var note = await noteBl.TrashNote(noteId, userId);
        if (note == null)
        {
            logger.LogWarning("Trashing failed, note {NoteId} not Trashed", noteId);
            return NotFound(new ResponseModel<string>()
            {
                Success = false, Message = "Note not found or could not be Trashed"
            });
        }

        logger.LogInformation("Trashed note {NoteId}", noteId);
        return Ok(new ResponseModel<NoteResponseDto>()
        {
            Success = true,
            Message = "Note Trashed",
            Data = note,
        });
    }

    /// <summary>
    /// Untrash note with given noteId
    /// </summary>
    /// <param name="noteId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{noteId:int}/untrash")]
    public async Task<IActionResult> UnTrashNote(int noteId)
    {
        logger.LogInformation("Untrashing note {NoteId}", noteId);
        var userId = (int)HttpContext.Items["userId"]!;
        
        var note = await noteBl.UnTrashNote(noteId, userId);
        if (note == null)
        {
            logger.LogWarning("Not untrashed note {NoteId}", noteId);
            return NotFound(new ResponseModel<string>()
            {
                Success = false, Message = "Note not found or could not be UnTrashed"
            });
        }

        logger.LogInformation("untrashed note {NoteId}", noteId);
        return Ok(new ResponseModel<NoteResponseDto>()
        {
            Success = true,
            Message = "Note UnTrashed",
            Data = note,
        });
    }

    /// <summary>
    /// Retrieve Trashed notes
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("trashed")]
    public async Task<IActionResult> GetTrashedNotes()
    {
        var userId = (int)HttpContext.Items["userId"]!;

        var notes = await noteBl.GetTrashedNotes(userId);

        logger.LogInformation("Retrieved Trashed notes");
        return Ok(new ResponseModel<List<NoteResponseDto>>
        {
            Success = true,
            Message = "Trashed notes retrieved successfully.",
            Data = notes
        });
    }

    // private (bool IsValid, int UserId) TryGetUserId()
    // {
    //     var claimValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
    //                      User.FindFirstValue("Sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    //     if (int.TryParse(claimValue, out var userId) && userId > 0)
    //     {
    //         logger.LogDebug("Got user {UserId} from token claims", userId);
    //         return (true, userId);
    //     }
    //
    //     logger.LogWarning("Failed getting user from token claims, token invalid");
    //     return (false, 0);
    // }
}

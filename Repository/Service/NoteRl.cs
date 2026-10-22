using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Context;
using Repository.Entity;
using Repository.Interface;

namespace Repository.Service;

public class NoteRl(FunDooDbContext dbContext, ILogger<NoteRl> logger) : INoteRl
{
    private readonly FunDooDbContext _dbContext = dbContext;

    public async Task<NoteEntity?> CreateAsync(NoteEntity note)
    {
        await _dbContext.Notes.AddAsync(note);
        var saved = await _dbContext.SaveChangesAsync();
        
        if (saved > 0) return note;
        logger.LogWarning("Failed to insert note with title '{Title}' for user {UserId}", note.Title, note.UserId);
        return null;
    }

    public async Task<bool> DeleteAsync(int noteId, int userId)
    {
        var result = await _dbContext.Notes.Where(u => u.Id == noteId && u.UserId == userId).ExecuteDeleteAsync();
        if (result > 0) return true;
        logger.LogWarning("note: {NoteId} deletion for userId: {UserId} failed", noteId, userId);
        return false;
    }

    public async Task<bool> UpdateAsync(NoteEntity note)
    {
        var result = await _dbContext.Notes
            .Where(u => u.Id == note.Id && u.UserId == note.UserId)
            .ExecuteUpdateAsync(n => n
                .SetProperty(entity => entity.Title, note.Title)
                .SetProperty(entity => entity.Description, note.Description));
        
        if (result > 0) return true;
        logger.LogWarning("Note {NoteId} update failed for user {UserId}", note.Id, note.UserId);
        return false;
    }

    public IEnumerable<NoteEntity> GetAllNotesByUser(int userId)
    {
        var notes = _dbContext.Notes
            .Where(n => n.UserId == userId && n.IsArchived == false)
            .ToList();
        return notes;
    }

    public async Task<NoteEntity?> GetNoteById(int noteId, int userId)
    {
        var note = await _dbContext.Notes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.UserId == userId && n.Id == noteId);
        if (note == null)
        {
            logger.LogWarning("note {NoteId} doesn't exist for {UserId}", noteId, userId);
        }
        return note;
    }

    public async Task<NoteEntity?> SetArchived(int noteId, int userId, bool isArchived)
    {
        var note = await _dbContext.Notes
            .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

        if (note is null)
        {
            logger.LogWarning("note {NoteId} doesn't exist for {UserId}", noteId, userId);
            return null;
        }

        note.IsArchived = isArchived;
        await _dbContext.SaveChangesAsync();
        return note;
    }

    public async Task<IEnumerable<NoteEntity>> GetArchived(int userId)
    {
        var notes = await _dbContext.Notes
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.IsArchived)
            .OrderByDescending(u => u.Id)
            .ToListAsync();
        
        return notes;
    }
}
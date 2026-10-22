using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Entity;
using Repository.Interface;

namespace Repository.Service;

public class NoteRl(FunDooDbContext dbContext) : INoteRl
{
    private readonly FunDooDbContext _dbContext = dbContext;

    public async Task<NoteEntity?> CreateAsync(NoteEntity note)
    {
        await _dbContext.Notes.AddAsync(note);
        var saved = await _dbContext.SaveChangesAsync();
        return saved > 0 ? note : null;
    }
    
    public async Task<bool> DeleteAsync(int noteId, int userId)
    {
        var result = await _dbContext.Notes.Where(u => u.Id == noteId && u.UserId == userId).ExecuteDeleteAsync();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(NoteEntity note)
    {
        var result = await _dbContext.Notes
            .Where(u => u.Id == note.Id && u.UserId == note.UserId)
            .ExecuteUpdateAsync(n => n
                .SetProperty(entity => entity.Title, note.Title)
                .SetProperty(entity => entity.Description, note.Description));
        return result > 0;
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

        return note;
    }

    public async Task<NoteEntity?> SetArchived(int noteId, int userId, bool isArchived)
    {
        var note = await _dbContext.Notes
            .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

        if (note is null) return null;

        note.IsArchived = isArchived;
        await _dbContext.SaveChangesAsync();
        return note;
    }

    public async Task<IEnumerable<NoteEntity>> GetArchived(int userId)
    {
        var notes = await _dbContext.Notes
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.IsArchived)
            .OrderByDescending(u=>u.Id)
            .ToListAsync();
        return notes;
    }
}

using Repository.Entity;

namespace Repository.Interface;

public interface INoteRl
{
    Task<NoteEntity?> CreateAsync(NoteEntity note);
    Task<bool> DeleteAsync(int noteId, int userId);
    Task<bool> UpdateAsync(NoteEntity note);
    IEnumerable<NoteEntity> GetAllNotesByUser(int userId);
    public Task<NoteEntity?> GetNoteById(int noteId, int userId);
    public Task<NoteEntity?> SetArchived(int noteId, int userId, bool isArchive);
    public Task<IEnumerable<NoteEntity>> GetArchived(int userId);
}

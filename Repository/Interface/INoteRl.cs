using Repository.Entity;

namespace Repository.Interface;

public interface INoteRl
{
    Task<NoteEntity?> CreateAsync(NoteEntity note);
    Task<bool> DeleteAsync(int noteId, int userId);
    Task<bool> UpdateAsync(NoteEntity note);
    Task<List<NoteEntity>> GetAllNotesByUser(int userId);
    Task<NoteEntity?> GetNoteById(int noteId, int userId);
    Task<NoteEntity?> SetArchived(int noteId, int userId, bool isArchive);
    Task<IEnumerable<NoteEntity>> GetArchived(int userId);
    Task<NoteEntity?> SetTrashed(int noteId, int userId, bool isTrashed);
    Task<IEnumerable<NoteEntity>> GetTrashed(int userId);
}

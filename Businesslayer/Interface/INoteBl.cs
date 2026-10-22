using ModelLayer.Model;

namespace BusinessLayer.Interface;

public interface INoteBl
{
    Task<NoteResponseDto?> CreateAsync(NoteRequestModel noteRequestModel, int userId);
    Task<bool> DeleteAsync(int noteId, int userId);
    Task<bool> UpdateAsync(NoteDomainModel noteDomainModel);
    Task<IEnumerable<NoteResponseDto>?> GetAllNotesByUser(int userId);
    public Task<NoteResponseDto?> GetNoteById(int noteId, int userId);
    public Task<NoteResponseDto?> ArchiveNote(int noteId, int userId);
    public Task<NoteResponseDto?> UnArchiveNote(int noteId, int userId);
    public Task<List<NoteResponseDto>> GetArchivedNotes(int userId);
}

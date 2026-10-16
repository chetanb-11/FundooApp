namespace Repository.Interface;

public interface INoteRl
{
    public Task<bool> Create(int userId);
    public Task<bool> Delete(int noteId, int userId);
}
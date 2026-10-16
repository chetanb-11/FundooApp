namespace Businesslayer.Interface;

public interface INoteBl
{
    public Task<bool> Create(string userId);
    public Task<bool> Delete(string noteId, string userId);
}
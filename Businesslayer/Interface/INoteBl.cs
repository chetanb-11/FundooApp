namespace Businesslayer.Interface;

public interface INoteBl
{
    public Task<bool> Create(string userId);
}
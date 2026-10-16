using Businesslayer.Interface;
using Repository.Interface;

namespace Businesslayer.Service;

public class NoteBl(IUserBl userBl, INoteRl noteRl) : INoteBl
{
    private readonly IUserBl _userBl = userBl;
    private readonly INoteRl _noteRl = noteRl;

    public async Task<bool> Create(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return false;

        int _userId = Convert.ToInt32(userId);
        var userExists = await _userBl.UserExistsByIdAsync(_userId);
        if (!userExists) return false;

        return await _noteRl.Create(userId);
    }
    public async Task<bool> Delete(string noteId, string userId)
    {
        int _noteId = Convert.ToInt32(noteId);
        int _userId = Convert.ToInt32(userId);
        if(string.IsNullOrWhiteSpace(userId, noteId)) return false;
        var userExists = await _noteBl.UserExistsByIdAsync(_userId);
        if(!userExists) return false;
        return await _noteRl.Delete(_noteId, _userId);
    }
}
using Businesslayer.Interface;
using Repository.Interface;

namespace Businesslayer.Service;

public class NoteBl(IUserBl userBl, INoteRl noteRl) : INoteBl
{
    private IUserBl _userBl = userBl;
    private INoteRl _noteRl = noteRl;
    
    public async Task<bool> Create(string userId)
    {
        var userExists = await _userBl.UserExistsByIdAsync(Convert.ToInt32(userId));
        if (!userExists) return false;
        
    }
}
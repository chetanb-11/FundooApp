using ModelLayer.Model;

namespace Repository.Interface;

public interface IUserRl
{
    public Task<UserDomainModel?> RegisterUser(UserDomainModel user);
    public Task<bool> UserExistsByEmail(string email);
    public Task<UserDomainModel?> FindExistsByEmail(string email);
    public Task<UserDomainModel?> LoginUser(string email);
    public Task<bool> SaveUser(UserDomainModel user);
}

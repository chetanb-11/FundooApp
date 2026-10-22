using ModelLayer.Model;

namespace Repository.Interface;

public interface IUserRl
{
    public Task<UserDomainModel?> RegisterUserAsync(UserDomainModel user);
    public Task<bool> UserExistsByEmail(string email);
    public Task<bool> UserExistsById(int id);
    public Task<UserDomainModel?> FindExistsByEmail(string email);
    public Task<UserDomainModel?> LoginUserAsync(string email);
    public Task<bool> SaveUser(UserDomainModel user);
}

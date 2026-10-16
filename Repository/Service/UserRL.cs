using Microsoft.EntityFrameworkCore;
using ModelLayer.Model;
using Repository.Context;
using Repository.Entity;
using Repository.Interface;

namespace Repository.Service;

public class UserRl : IUserRl
{
    private readonly FunDooDbContext _dbContext;

    public UserRl(FunDooDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDomainModel?> RegisterUser(UserDomainModel user)
    {
        var entity = ToEntity(user);

        await _dbContext.Users.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return ToDomain(entity);
    }

    public async Task<UserDomainModel?> LoginUser(string email)
    {
        var entity =
            await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return entity == null ? null : ToDomain(entity);
    }

    public async Task<bool> UserExistsById(int id)
    {
        var entity = await _dbContext.Users.AnyAsync(u => u.Id == id);
        return entity;
    }

    public async Task<UserDomainModel?> FindExistsByEmail(string email)
    {
        var entity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return entity == null ? null : ToDomain(entity);
    }

    public async Task<bool> UserExistsByEmail(string email) =>
        await _dbContext.Users.AnyAsync(u => u.Email == email);

    public async Task<bool> SaveUser(UserDomainModel user)
    {
        var entity = await _dbContext.Users.FindAsync(user.Id);
        if (entity == null) return false;

        entity.FirstName = user.FirstName;
        entity.LastName = user.LastName;
        entity.Email = user.Email;
        entity.ContactNumber = user.ContactNumber;
        entity.Password = user.Password;

        return await _dbContext.SaveChangesAsync() > 0;
    }


    private static UserEntity ToEntity(UserDomainModel model)
    {
        return new UserEntity
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            ContactNumber = model.ContactNumber,
            Password = model.Password,
        };
    }

    private static UserDomainModel? ToDomain(UserEntity? entity)
    {
        if (entity == null) return null;
        return new UserDomainModel
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            ContactNumber = entity.ContactNumber,
            Password = entity.Password,
        };
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLayer.Model;
using Repository.Context;
using Repository.Entity;
using Repository.Interface;

namespace Repository.Service;

public class UserRl(FunDooDbContext dbContext, ILogger<UserRl> logger) : IUserRl
{
    private readonly FunDooDbContext _dbContext = dbContext;
    private readonly ILogger<UserRl> _logger = logger;

    public async Task<UserDomainModel?> RegisterUserAsync(UserDomainModel user)
    {
        var entity = ToEntity(user);

        try
        {
            await _dbContext.Users.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return ToDomain(entity);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error occurred while registering user {Email}", user.Email);
            return null; 
        }
    }

    public async Task<UserDomainModel?> LoginUserAsync(string email)
    {
        var entity =
            await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        return entity == null ? null : ToDomain(entity);
    }

    public async Task<bool> UserExistsById(int id)
    {
        var entity = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == id);
        return entity;
    }

    public async Task<UserDomainModel?> FindExistsByEmail(string email)
    {
        var entity = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
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
        entity.Salt = user.Salt;

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
            Salt = model.Salt
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
            Salt = entity.Salt
        };
    }
}

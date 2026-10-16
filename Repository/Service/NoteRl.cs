using Repository.Context;
using Repository.Interface;

namespace Repository.Service;

public class NoteRl(FunDooDbContext dbContext) : INoteRl
{
    private readonly FunDooDbContext _dbContext = dbContext;

    public async Task<bool> Create(int userId)
    {
        return true;
    }
    public async Task<bool> Delete(int noteId, int userId)
    {
        return true;
    }
}
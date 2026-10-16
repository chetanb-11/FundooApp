using Repository.Context;
using Repository.Interface;

namespace Repository.Service;

public class NoteRl(FunDooDbContext dbContext) : INoteRl
{
    private FunDooDbContext _dbContext = dbContext;
    
    public string Create()
    {
        var result = _dbContext.Notes.AddAsync()
    }
}
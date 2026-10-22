using Microsoft.EntityFrameworkCore;
using ModelLayer.Model;
using Repository.Entity;

namespace Repository.Context;

public class FunDooDbContext : DbContext
{
    public FunDooDbContext(DbContextOptions<FunDooDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<NoteEntity> Notes => Set<NoteEntity>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FunDooDbContext).Assembly);

        modelBuilder.Entity<NoteEntity>(e =>
        {
            e.Property(n => n.IsArchived).HasDefaultValue(false);
            e.Property(n => n.IsTrashed).HasDefaultValue(false);
        });
        
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}

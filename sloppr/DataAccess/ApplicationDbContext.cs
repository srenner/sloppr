using Microsoft.EntityFrameworkCore;
using sloppr.Models;

namespace sloppr.DataAccess;

public class ApplicationDbContext : DbContext
{
    public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
    public DbSet<KeyIngredient> KeyIngredients { get; set; }
    public DbSet<Cuisine> Cuisines { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<AiProvider> AiProviders { get; set; }
    public DbSet<AiModel> AiModels { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public override int SaveChanges()
    {
        StampAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAuditFields()
    {
        DateTime now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseModel>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.DateCreated = now;
                    entry.Entity.DateUpdated = now;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(BaseModel.DateCreated)).IsModified = false;
                    entry.Entity.DateUpdated = now;
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region Indexes

        modelBuilder.Entity<KeyIngredient>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Cuisine>()
            .HasIndex(x => x.Name)
            .IsUnique();

        #endregion

        #region Query filters

        modelBuilder.Entity<AiProvider>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<AiModel>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);

        #endregion

        #region Check constraints

        modelBuilder.Entity<ApplicationSetting>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable(t => t.HasCheckConstraint("CK_Settings_SingleRow", "[Id] = 1"));
            entity.HasData(new ApplicationSetting());
        });

        #endregion
    }
}

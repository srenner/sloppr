using Microsoft.EntityFrameworkCore;
using sloppr.Models;
using sloppr.Recipes;

namespace sloppr.DataAccess;

public class ApplicationDbContext : DbContext
{
    public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
    public DbSet<KeyIngredient> KeyIngredients { get; set; }
    public DbSet<Cuisine> Cuisines { get; set; }
    public DbSet<MealIdea> MealIdeas { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<RecipeStep> RecipeSteps { get; set; }
    public DbSet<RecipeFormatVersion> RecipeFormats { get; set; }
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

        #region Relationships

        modelBuilder.Entity<MealIdea>()
            .HasMany(m => m.KeyIngredients)
            .WithMany()
            .UsingEntity<MealIdeaKeyIngredient>(
                j => j.HasOne<KeyIngredient>().WithMany().HasForeignKey(ji => ji.KeyIngredientId),
                j => j.HasOne<MealIdea>().WithMany().HasForeignKey(ji => ji.MealIdeaId),
                j =>
                {
                    j.ToTable("MealIdeaKeyIngredients");
                    j.HasKey(ji => new { ji.MealIdeaId, ji.KeyIngredientId });
                });

        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Ingredients)
            .WithOne()
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Steps)
            .WithOne()
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Recipe>()
            .HasOne<RecipeFormatVersion>()
            .WithMany()
            .HasForeignKey(r => new { r.FormatKey, r.FormatVersion })
            .HasPrincipalKey(f => new { f.Key, f.Version })
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Recipe>()
            .Property(r => r.FormatKey)
            .HasDefaultValue(RecipeFormat.Key);

        modelBuilder.Entity<Recipe>()
            .Property(r => r.FormatVersion)
            .HasDefaultValue(RecipeFormat.Version);

        #endregion

        #region Query filters

        modelBuilder.Entity<AiProvider>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<AiModel>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<MealIdea>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<Recipe>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);

        #endregion

        #region Check constraints

        modelBuilder.Entity<RecipeFormatVersion>(entity =>
        {
            entity.ToTable("RecipeFormats");
            entity.HasAlternateKey(f => new { f.Key, f.Version });
            entity.HasData(new RecipeFormatVersion
            {
                Id = 1,
                Key = RecipeFormat.Key,
                Version = RecipeFormat.Version,
                Rules = RecipeFormat.StandardV1RulesJson,
                IsCurrent = true,
                DateCreated = RecipeFormat.SeedDate,
                CreatedBy = "system"
            });
        });

        modelBuilder.Entity<ApplicationSetting>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable(t => t.HasCheckConstraint("CK_Settings_SingleRow", "[Id] = 1"));
            entity.HasData(new ApplicationSetting());
        });

        #endregion
    }
}

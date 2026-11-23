using Microsoft.EntityFrameworkCore;
using sloppr.Models;

namespace sloppr.DataAccess;

public class ApplicationDbContext : DbContext
{
    public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
    public DbSet<AiCapabilitySetting> AiCapabilitySettings { get; set; }
    public DbSet<KeyIngredient> KeyIngredients { get; set; }
    public DbSet<Cuisine> Cuisines { get; set; }
    public DbSet<MealIdea> MealIdeas { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeTemplate> RecipeTemplates { get; set; }
    public DbSet<AiProvider> AiProviders { get; set; }
    public DbSet<AiModel> AiModels { get; set; }
    public DbSet<Cookbook> Cookbooks { get; set; }
    public DbSet<CookbookEntry> CookbookEntries { get; set; }
    public DbSet<CookbookEntryVersion> CookbookEntryVersions { get; set; }

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

        modelBuilder.Entity<RecipeTemplate>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Cookbook>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<CookbookEntry>()
            .HasIndex(x => x.CookbookId);

        modelBuilder.Entity<CookbookEntry>()
            .HasIndex(x => x.SourceRecipeId);

        modelBuilder.Entity<CookbookEntryVersion>()
            .HasIndex(x => new { x.CookbookEntryId, x.VersionNumber })
            .IsUnique();

        modelBuilder.Entity<AiCapabilitySetting>()
            .HasIndex(x => x.Capability)
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

        modelBuilder.Entity<Cookbook>()
            .HasMany(c => c.Entries)
            .WithOne(e => e.Cookbook)
            .HasForeignKey(e => e.CookbookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CookbookEntry>()
            .HasOne(e => e.SourceRecipe)
            .WithMany()
            .HasForeignKey(e => e.SourceRecipeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CookbookEntry>()
            .HasMany(e => e.Versions)
            .WithOne(v => v.CookbookEntry)
            .HasForeignKey(v => v.CookbookEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AiCapabilitySetting>()
            .HasOne<AiModel>()
            .WithMany()
            .HasForeignKey(s => s.ModelId)
            .OnDelete(DeleteBehavior.SetNull);

        #endregion

        #region Query filters

        modelBuilder.Entity<AiProvider>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<AiModel>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<MealIdea>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<Recipe>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<Cookbook>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<CookbookEntry>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);

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

using Microsoft.EntityFrameworkCore;
using sloppr.Models;

namespace sloppr.DataAccess;

public class ApplicationDbContext : DbContext
{
    public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
    public DbSet<KeyIngredient> KeyIngredients { get; set; }
    public DbSet<Cuisine> Cuisines { get; set; }
    public DbSet<MealIdea> MealIdeas { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<AiProvider> AiProviders { get; set; }
    public DbSet<AiModel> AiModels { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
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

        #endregion

        #region Query filters

        modelBuilder.Entity<AiProvider>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<AiModel>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);
        modelBuilder.Entity<MealIdea>().HasQueryFilter(f => f.IsActive && !f.IsDeleted);

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

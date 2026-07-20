using Microsoft.EntityFrameworkCore;
using sloppr.Models;

namespace sloppr.DataAccess;

public class ApplicationDbContext : DbContext
{
    public DbSet<KeyIngredient> KeyIngredients { get; set; }
    public DbSet<Recipe> Recipes { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}

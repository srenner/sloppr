using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Services;

public class KeyIngredientService : IKeyIngredientService
{
    private readonly IUnitOfWork _uow;
    private readonly ApplicationDbContext _db;

    public KeyIngredientService(IUnitOfWork uow, ApplicationDbContext db)
    {
        _uow = uow;
        _db = db;
    }

    public async Task AddAsync(KeyIngredient ingredient)
    {
        await _uow.Repository<KeyIngredient>().AddAsync(ingredient);
        await _uow.CompleteAsync();
    }

    public async Task<IEnumerable<KeyIngredient>> GetAllAsync()
    {
        return await _uow.Repository<KeyIngredient>().GetAllAsync();
    }

    public async Task<KeyIngredient?> GetByIdAsync(int id)
    {
        return await _uow.Repository<KeyIngredient>().GetByIdAsync(id);
    }

    public async Task<KeyIngredient> UpdateAsync(KeyIngredient ingredient)
    {
        _uow.Repository<KeyIngredient>().Update(ingredient);
        await _uow.CompleteAsync();
        return ingredient;
    }

    public async Task<ICollection<KeyIngredientDTO>> Upsert(string[] names)
    {
        var dtos = new List<KeyIngredientDTO>();
        foreach (var name in names)
        {
            string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var ingredient = await _db.Database
                .SqlQuery<KeyIngredientDTO>($"""
                    INSERT INTO KeyIngredients (Name, NumQueried, CreatedBy, DateCreated, UpdatedBy, DateUpdated, IsActive, IsDeleted, NumUsed)
                    VALUES ({name}, 1, 'system', {now}, 'system', {now}, 1, 0, 0)
                    ON CONFLICT(Name)
                    DO UPDATE SET NumQueried = NumQueried + 1, UpdatedBy = 'system', DateUpdated = {now}
                    RETURNING Id, Name, NumQueried, CreatedBy, DateCreated, UpdatedBy, DateUpdated, IsActive, IsDeleted, NumUsed, LastUsed;
                    """)
                .AsAsyncEnumerable()
                .SingleAsync();
            dtos.Add(ingredient);
        }
        return dtos;
    }
}

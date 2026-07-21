using System;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using sloppr.DataAccess;
using sloppr.Models;

namespace sloppr.Services;

public class KeyIngredientService : IKeyIngredientService
{
    private readonly IUnitOfWork _uow;
    public KeyIngredientService(IUnitOfWork uow) => _uow = uow;

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
}

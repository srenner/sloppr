using System;
using sloppr.Models;

namespace sloppr.Services;

public interface IKeyIngredientService
{
    public Task AddAsync(KeyIngredient ingredient);
    public Task<KeyIngredient?> GetByIdAsync(int id);
}

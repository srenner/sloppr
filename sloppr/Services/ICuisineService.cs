using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Services;

public interface ICuisineService
{
    public Task AddAsync(Cuisine cuisine);
    public Task<Cuisine?> GetByIdAsync(int id);
    public Task<IEnumerable<Cuisine>> GetAllAsync();
    public Task<Cuisine> UpdateAsync(Cuisine cuisine);
    public Task<ICollection<CuisineDTO>> Upsert(string[] names);
}

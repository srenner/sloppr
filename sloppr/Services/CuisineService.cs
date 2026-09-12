using Microsoft.EntityFrameworkCore;
using sloppr.DataAccess;
using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Services;

public class CuisineService : ICuisineService
{
    private readonly IUnitOfWork _uow;
    private readonly ApplicationDbContext _db;

    public CuisineService(IUnitOfWork uow, ApplicationDbContext db)
    {
        _uow = uow;
        _db = db;
    }

    public async Task AddAsync(Cuisine cuisine)
    {
        await _uow.Repository<Cuisine>().AddAsync(cuisine);
        await _uow.CompleteAsync();
    }

    public async Task<IEnumerable<Cuisine>> GetAllAsync()
    {
        return await _uow.Repository<Cuisine>().GetAllAsync();
    }

    public async Task<Cuisine?> GetByIdAsync(int id)
    {
        return await _uow.Repository<Cuisine>().GetByIdAsync(id);
    }

    public async Task<Cuisine> UpdateAsync(Cuisine cuisine)
    {
        _uow.Repository<Cuisine>().Update(cuisine);
        await _uow.CompleteAsync();
        return cuisine;
    }

    public async Task<ICollection<CuisineDTO>> Upsert(string[] names)
    {
        var dtos = new List<CuisineDTO>();
        foreach (var name in names)
        {
            string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var cuisine = await _db.Database
                .SqlQuery<CuisineDTO>($"""
                    INSERT INTO Cuisines (Name, NumQueried, CreatedBy, DateCreated, UpdatedBy, DateUpdated, IsActive, IsDeleted, NumUsed)
                    VALUES ({name}, 1, 'system', {now}, 'system', {now}, 1, 0, 0)
                    ON CONFLICT(Name)
                    DO UPDATE SET NumQueried = NumQueried + 1, UpdatedBy = 'system', DateUpdated = {now}
                    RETURNING Id, Name, NumQueried, CreatedBy, DateCreated, UpdatedBy, DateUpdated, IsActive, IsDeleted, NumUsed, LastUsed;
                    """)
                .AsAsyncEnumerable()
                .SingleAsync();
            dtos.Add(cuisine);
        }
        return dtos;
    }
}

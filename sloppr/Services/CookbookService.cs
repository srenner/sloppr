using Microsoft.EntityFrameworkCore;
using sloppr.Content;
using sloppr.DataAccess;
using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Services;

public class CookbookService : ICookbookService
{
    private readonly ApplicationDbContext _db;

    public CookbookService(ApplicationDbContext db) => _db = db;

    public async Task EnsureSeededAsync()
    {
        if (await _db.Cookbooks.AnyAsync())
        {
            return;
        }

        _db.Cookbooks.Add(new Cookbook { Name = "Family Cookbook" });
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<CookbookDTO>> GetCookbooksAsync()
    {
        return await _db.Cookbooks
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CookbookDTO
            {
                Id = c.Id,
                Name = c.Name,
                DateCreated = c.DateCreated,
                DateUpdated = c.DateUpdated
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CookbookEntrySummaryDTO>?> GetEntriesAsync(int cookbookId)
    {
        if (!await _db.Cookbooks.AnyAsync(c => c.Id == cookbookId))
        {
            return null;
        }

        return await (from entry in _db.CookbookEntries
                      where entry.CookbookId == cookbookId
                      join version in _db.CookbookEntryVersions
                          on new { EntryId = entry.Id, Version = entry.CurrentVersionNumber }
                          equals new { EntryId = version.CookbookEntryId, Version = version.VersionNumber }
                      orderby version.Name
                      select new CookbookEntrySummaryDTO
                      {
                          Id = entry.Id,
                          CookbookId = entry.CookbookId,
                          SourceRecipeId = entry.SourceRecipeId,
                          CurrentVersionNumber = entry.CurrentVersionNumber,
                          Name = version.Name,
                          Description = version.Description,
                          ServingCount = version.ServingCount,
                          PrepTimeMinutes = version.PrepTimeMinutes,
                          CookTimeMinutes = version.CookTimeMinutes,
                          Difficulty = version.Difficulty,
                          DateCreated = entry.DateCreated,
                          DateUpdated = entry.DateUpdated
                      })
                     .ToListAsync();
    }

    public async Task<CookbookEntryDTO?> GetEntryAsync(int cookbookId, int entryId, int? version)
    {
        var entry = await _db.CookbookEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == entryId && e.CookbookId == cookbookId);

        if (entry == null)
        {
            return null;
        }

        var versionNumber = version ?? entry.CurrentVersionNumber;
        var snapshot = await _db.CookbookEntryVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.CookbookEntryId == entryId && v.VersionNumber == versionNumber);

        return snapshot == null ? null : ToDto(entry, snapshot);
    }

    public async Task<CookbookEntryDTO?> CreateEntryAsync(int cookbookId, CookbookEntryRequest request)
    {
        if (!await _db.Cookbooks.AnyAsync(c => c.Id == cookbookId))
        {
            return null;
        }

        RecipeContentJson.Normalize(request.Content);

        var entry = new CookbookEntry
        {
            CookbookId = cookbookId,
            CurrentVersionNumber = 1
        };
        entry.Versions.Add(CreateVersion(1, request.Content, request.ChangeNote));

        _db.CookbookEntries.Add(entry);
        await _db.SaveChangesAsync();

        return ToDto(entry, entry.Versions.First());
    }

    public async Task<CookbookCopyResult?> CopyFromRecipeAsync(int cookbookId, int recipeId)
    {
        if (!await _db.Cookbooks.AnyAsync(c => c.Id == cookbookId))
        {
            return null;
        }

        var recipe = await _db.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        if (recipe == null)
        {
            return null;
        }

        var existing = await _db.CookbookEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CookbookId == cookbookId && e.SourceRecipeId == recipeId);

        if (existing != null)
        {
            var existingVersion = await _db.CookbookEntryVersions
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.CookbookEntryId == existing.Id && v.VersionNumber == existing.CurrentVersionNumber);

            if (existingVersion != null)
            {
                return new CookbookCopyResult(ToDto(existing, existingVersion), false);
            }
        }

        var content = RecipeContentJson.Deserialize(recipe.ContentJson);

        var entry = new CookbookEntry
        {
            CookbookId = cookbookId,
            SourceRecipeId = recipe.Id,
            CurrentVersionNumber = 1
        };
        entry.Versions.Add(CreateVersion(1, content, null));

        _db.CookbookEntries.Add(entry);
        await _db.SaveChangesAsync();

        return new CookbookCopyResult(ToDto(entry, entry.Versions.First()), true);
    }

    public async Task<CookbookEntryDTO?> UpdateEntryAsync(int cookbookId, int entryId, CookbookEntryRequest request)
    {
        var entry = await _db.CookbookEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.CookbookId == cookbookId);

        if (entry == null)
        {
            return null;
        }

        RecipeContentJson.Normalize(request.Content);

        var versionNumber = entry.CurrentVersionNumber + 1;
        var version = CreateVersion(versionNumber, request.Content, request.ChangeNote);
        entry.Versions.Add(version);
        entry.CurrentVersionNumber = versionNumber;

        await _db.SaveChangesAsync();

        return ToDto(entry, version);
    }

    public async Task<bool> DeleteEntryAsync(int cookbookId, int entryId)
    {
        var entry = await _db.CookbookEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.CookbookId == cookbookId);

        if (entry == null)
        {
            return false;
        }

        entry.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }

    private static CookbookEntryVersion CreateVersion(int versionNumber, RecipeContent content, string? changeNote)
    {
        return new CookbookEntryVersion
        {
            VersionNumber = versionNumber,
            ContentJson = RecipeContentJson.Serialize(content),
            Name = content.Name,
            Description = content.Description,
            ServingCount = content.ServingCount,
            PrepTimeMinutes = content.PrepTimeMinutes,
            CookTimeMinutes = content.CookTimeMinutes,
            Difficulty = content.Difficulty,
            ChangeNote = changeNote
        };
    }

    private static CookbookEntryDTO ToDto(CookbookEntry entry, CookbookEntryVersion version)
    {
        return new CookbookEntryDTO
        {
            Id = entry.Id,
            CookbookId = entry.CookbookId,
            SourceRecipeId = entry.SourceRecipeId,
            CurrentVersionNumber = version.VersionNumber,
            Content = RecipeContentJson.Deserialize(version.ContentJson),
            DateCreated = entry.DateCreated,
            DateUpdated = entry.DateUpdated
        };
    }
}

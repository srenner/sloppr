using Microsoft.AspNetCore.Mvc;
using sloppr.Content;
using sloppr.DTOs;
using sloppr.Services;

namespace sloppr.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CookbooksController(ICookbookService svc) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CookbookDTO>>> GetCookbooksAsync()
    {
        return Ok(await svc.GetCookbooksAsync());
    }

    [HttpGet("{cookbookId:int}/entries")]
    public async Task<ActionResult<IReadOnlyList<CookbookEntrySummaryDTO>>> GetEntries(int cookbookId)
    {
        var entries = await svc.GetEntriesAsync(cookbookId);
        return entries == null ? NotFound() : Ok(entries);
    }

    [HttpGet("{cookbookId:int}/entries/{entryId:int}")]
    public async Task<ActionResult<CookbookEntryDTO>> GetEntry(
        int cookbookId,
        int entryId,
        [FromQuery] int? version = null)
    {
        var entry = await svc.GetEntryAsync(cookbookId, entryId, version);
        return entry == null ? NotFound() : Ok(entry);
    }

    [HttpPost("{cookbookId:int}/entries")]
    public async Task<ActionResult<CookbookEntryDTO>> CreateEntryAsync(
        int cookbookId,
        CookbookEntryRequest request)
    {
        if (request.Content == null)
        {
            return BadRequest("Content is required.");
        }

        var errors = RecipeContentJson.Validate(request.Content);
        if (errors.Count > 0)
        {
            return ValidationProblemWith(errors);
        }

        var entry = await svc.CreateEntryAsync(cookbookId, request);
        if (entry == null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetEntry), new { cookbookId, entryId = entry.Id }, entry);
    }

    [HttpPost("{cookbookId:int}/entries/from-recipe/{recipeId:int}")]
    public async Task<ActionResult<CookbookEntryDTO>> CopyFromRecipeAsync(int cookbookId, int recipeId)
    {
        var entry = await svc.CopyFromRecipeAsync(cookbookId, recipeId);
        if (entry == null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetEntry), new { cookbookId, entryId = entry.Id }, entry);
    }

    [HttpPut("{cookbookId:int}/entries/{entryId:int}")]
    public async Task<ActionResult<CookbookEntryDTO>> UpdateEntryAsync(
        int cookbookId,
        int entryId,
        CookbookEntryRequest request)
    {
        if (request.Content == null)
        {
            return BadRequest("Content is required.");
        }

        var errors = RecipeContentJson.Validate(request.Content);
        if (errors.Count > 0)
        {
            return ValidationProblemWith(errors);
        }

        var entry = await svc.UpdateEntryAsync(cookbookId, entryId, request);
        return entry == null ? NotFound() : Ok(entry);
    }

    [HttpDelete("{cookbookId:int}/entries/{entryId:int}")]
    public async Task<IActionResult> DeleteEntryAsync(int cookbookId, int entryId)
    {
        return await svc.DeleteEntryAsync(cookbookId, entryId) ? NoContent() : NotFound();
    }

    private ActionResult ValidationProblemWith(List<string> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError("content", error);
        }

        return ValidationProblem(ModelState);
    }
}

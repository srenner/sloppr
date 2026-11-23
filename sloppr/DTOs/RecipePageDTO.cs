namespace sloppr.DTOs;

public class RecipePageDTO
{
    public IReadOnlyList<RecipeSummaryDTO> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

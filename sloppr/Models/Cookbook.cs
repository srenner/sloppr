namespace sloppr.Models;

public class Cookbook : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public ICollection<CookbookEntry> Entries { get; set; } = [];
}

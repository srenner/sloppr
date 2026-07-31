namespace sloppr.AI.Extraction;

public interface IModelClient
{
    Task<string> GetCompletionAsync(string prompt);
}

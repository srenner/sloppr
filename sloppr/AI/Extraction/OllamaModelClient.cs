using System.Text.Json;

namespace sloppr.AI.Extraction;

public class OllamaModelClient(string model) : IModelClient
{
    private readonly HttpClient _http = new();
    private readonly string _model = model;

    public async Task<string> GetCompletionAsync(string prompt)
    {
        var body = new
        {
            model = _model,
            stream = false,
            messages = new[] { new { role = "user", content = prompt } }
        };

        var response = await _http.PostAsJsonAsync("http://localhost:11434/api/chat", body);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("message").GetProperty("content").GetString() ?? "";
    }
}

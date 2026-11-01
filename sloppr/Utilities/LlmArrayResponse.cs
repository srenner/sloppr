using System.Text.Json;

namespace sloppr.Utilities;

/// <summary>
/// Parses a JSON string containing a single-key object with an array value,
/// such as <c>{"ingredients":["red onions","peppers"]}</c>, and extracts the array.
/// Returns an empty list if parsing fails or the input is null/empty.
/// </summary>
internal static class LlmArrayResponse
{
    public static List<string> Read(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
            return parsed?.Values.FirstOrDefault() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

using Scalar.AspNetCore.Attributes;

namespace sloppr.AI.Extraction;

[Deprecated]
public class ExtractionEvaluator(IModelClient client)
{
    private readonly IModelClient _client = client;

    public async Task RunAsync()
    {
        var cases = new (string prompt, string expected)[]
        {
            ("Extract the email from: Contact John at john@x.com", "john@x.com"),
            ("Extract the date from: The meeting is on 2024-05-01", "2024-05-01"),
            ("Extract the phone number from: Call me at 555-1234", "555-1234"),
        };

        int passed = 0;
        foreach (var (prompt, expected) in cases)
        {
            var actual = await _client.GetCompletionAsync(prompt);
            var ok = actual.Contains(expected);
            Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] expected='{expected}' actual='{actual.Trim()}'");
            if (ok) passed++;
        }
        Console.WriteLine($"\nScore: {passed}/{cases.Length}");

    }
}

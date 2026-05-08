using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal sealed class FailureWriter(string failuresDir)
{
    public void WriteAndThrow(
        string operation,
        DiffInput input,
        string reason,
        RunResult ours,
        RunResult theirs)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input.RawInput)))[..16];
        var fileName = $"failure_{SanitizeFileName(operation)}_{hash}.json";
        var path = Path.Combine(failuresDir, fileName);

        var failure = new Failure(operation, reason, input, ours, theirs);

        File.WriteAllText(
            path,
            JsonSerializer.Serialize(failure, new JsonSerializerOptions { WriteIndented = true }));

        throw new InvalidOperationException($"Differential mismatch in {operation}: {reason}. Repro saved to {path}");
    }

    private static string SanitizeFileName(string value)
    {
        return Regex.Replace(value, @"[^a-zA-Z0-9_.-]", "_");
    }
}

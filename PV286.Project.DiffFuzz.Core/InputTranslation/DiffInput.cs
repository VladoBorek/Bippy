using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NBitcoin;

internal sealed record DiffInput(
    string RawInput,
    Operation Operation,
    string EntropyHex,
    string Phrase,
    string SeedHex,
    string WrongSeedHex,
    string Path)
{
    public byte[] EntropyBytes => Convert.FromHexString(EntropyHex);

    public byte[] SeedBytes => Convert.FromHexString(SeedHex);

    public byte[] WrongSeedBytes => Convert.FromHexString(WrongSeedHex);

    private static readonly string[] ValidPaths =
    [
        "m",
        "m/0",
        "m/0/1",
        "m/0'/1",
        "m/44'/0'/0'/0/0",
        "m/84'/0'/0'/0/0"
    ];

    public static DiffInput FromRaw(string rawInput, bool includeDerive, bool includeDeriveEntropy)
    {
        var fields = ParseFields(rawInput);
        var hash = SHA512.HashData(Encoding.UTF8.GetBytes(rawInput));

        var operation = SelectOperation(fields, hash, includeDerive, includeDeriveEntropy);
        var entropy = SelectEntropy(fields, rawInput, hash);
        var entropyHex = Convert.ToHexString(entropy).ToLowerInvariant();

        var mnemonic = new NBitcoin.Mnemonic(NBitcoin.Wordlist.English, entropy);

        var phrase = mnemonic.ToString();
        var seedHex = Convert.ToHexString(mnemonic.DeriveSeed()).ToLowerInvariant();

        var path = fields.TryGetValue("path", out var parsedPath) && ValidPaths.Contains(parsedPath)
            ? parsedPath
            : ValidPaths[hash[1] % ValidPaths.Length];

        return new DiffInput(
            rawInput,
            operation,
            entropyHex,
            phrase,
            seedHex,
            FlipFirstHexDigit(seedHex),
            path);
    }

    private static Dictionary<string, string> ParseFields(string rawInput)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in rawInput.Replace("\r\n", "\n").Split('\n'))
        {
            var separator = line.IndexOf('=');
            if (separator <= 0) continue;

            fields[line[..separator].Trim()] = line[(separator + 1)..].Trim();
        }

        return fields;
    }

    private static Operation SelectOperation(
        Dictionary<string, string> fields,
        byte[] hash,
        bool includeDerive,
        bool includeDeriveEntropy)
    {
        if (fields.TryGetValue("op", out var op))
        {
            var parsed = op.Trim().ToLowerInvariant() switch
            {
                "encode" or "generate" => Operation.Encode,
                "decode" or "recover" => Operation.Decode,
                "verify-ok" => Operation.VerifyOk,
                "verify-nok" => Operation.VerifyNok,
                "derive-seed" => includeDerive ? Operation.DeriveSeed : (Operation?)null,
                "derive-entropy" => includeDeriveEntropy ? Operation.DeriveEntropy : (Operation?)null,
                _ => null
            };

            if (parsed is not null)
            {
                return parsed.Value;
            }
        }

        var operations = new List<Operation>
        {
            Operation.Encode,
            Operation.Decode,
            Operation.VerifyOk,
            Operation.VerifyNok
        };

        if (includeDerive) operations.Add(Operation.DeriveSeed);
        if (includeDeriveEntropy) operations.Add(Operation.DeriveEntropy);

        return operations[hash[0] % operations.Count];
    }

    private static byte[] SelectEntropy(Dictionary<string, string> fields, string rawInput, byte[] hash)
    {
        if (fields.TryGetValue("entropy", out var entropyHex) && TryParseEntropyHex(entropyHex, out var entropy))
        {
            return entropy;
        }

        if (TryParseEntropyHex(rawInput.Trim(), out entropy))
        {
            return entropy;
        }

        var lengths = new[] { 16, 20, 24, 28, 32 };
        return hash.Take(lengths[hash[2] % lengths.Length]).ToArray();
    }

    private static bool TryParseEntropyHex(string value, out byte[] entropy)
    {
        entropy = Array.Empty<byte>();
        value = value.Trim();

        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            value = value[2..];
        }

        if (value.Length % 2 != 0 || !Regex.IsMatch(value, @"\A[0-9a-fA-F]+\z"))
        {
            return false;
        }

        var byteLength = value.Length / 2;

        if (byteLength is not (16 or 20 or 24 or 28 or 32))
        {
            return false;
        }

        entropy = Convert.FromHexString(value);
        return true;
    }

    private static string FlipFirstHexDigit(string seedHex)
    {
        var chars = seedHex.ToCharArray();
        chars[0] = chars[0] == '0' ? '1' : '0';
        return new string(chars);
    }
}

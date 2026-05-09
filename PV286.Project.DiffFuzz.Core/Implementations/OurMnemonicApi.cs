using BusinessLayer.Cli.Utils.Enums;
using BusinessLayer.Services;
using ResultPattern;

internal sealed class OurMnemonicApi
{
    private readonly EncodeService encode = new();
    private readonly DecodeService decode = new();
    private readonly VerifyService verify = new();
    private readonly DeriveService derive = new();

    public GeneratedMnemonicResult GenerateFromEntropy(byte[] entropy)
    {
        var result = Require(encode.Encode(entropy, ValueFormat.Hex), "ours encode");

        return new GeneratedMnemonicResult(
            Convert.ToHexString(entropy).ToLowerInvariant(),
            result.Mnemonic.ToString(),
            Convert.ToHexString(result.Seed).ToLowerInvariant());
    }

    public GeneratedMnemonicResult RecoverFromPhrase(string phrase)
    {
        var result = Require(decode.Decode(phrase, ValueFormat.Hex), "ours decode");

        return new GeneratedMnemonicResult(
            Convert.ToHexString(result.Entropy).ToLowerInvariant(),
            result.Mnemonic.ToString(),
            Convert.ToHexString(result.Seed).ToLowerInvariant());
    }

    public string Verify(string phrase, byte[] seed)
    {
        var result = Require(verify.Verify(phrase, seed), "ours verify");
        return result.IsValid ? "OK" : "NOK";
    }

    public DerivedKeysResult DeriveFromSeed(byte[] seed, string? path)
    {
        var result = Require(derive.Derive(seed, path), "ours derive seed");

        return new DerivedKeysResult(
            result.ExtendedPrivateKey,
            result.ExtendedPublicKey);
    }

    public DerivedKeysResult DeriveFromEntropy(byte[] entropy, string? path)
    {
        var mnemonic = new NBitcoin.Mnemonic(NBitcoin.Wordlist.English, entropy);
        var seed = mnemonic.DeriveSeed();

        var result = Require(derive.Derive(seed, path), "ours derive entropy");

        return new DerivedKeysResult(
            result.ExtendedPrivateKey,
            result.ExtendedPublicKey);
    }


    private static T Require<T>(Result<T> result, string operation)
    {
        if (result.IsFailed)
        {
            throw new InvalidOperationException($"{operation} failed: {result.Error}");
        }

        return result.Value;
    }
}

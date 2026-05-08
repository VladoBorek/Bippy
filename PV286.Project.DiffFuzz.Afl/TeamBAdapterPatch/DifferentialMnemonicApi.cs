using Microsoft.Extensions.DependencyInjection;
using Mnemonic.Interfaces;
using Mnemonic.Records;

namespace Mnemonic;

// Copy this file to:
// Project_TEAM_B/team-34/Mnemonic/Mnemonic/DifferentialMnemonicApi.cs
//
// Required only for the in-process differential fuzzing harness.
// It exposes Team B's internal BIP39/BIP32 services through a stable adapter API.


public sealed class DifferentialMnemonicApi
{
    private readonly IBip39Service bip39;
    private readonly IBip32Service bip32;

    public DifferentialMnemonicApi()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IBip39Service, Services.Bip39Service>();
        services.AddSingleton<IBip32Service, Services.Bip32Service>();

        var provider = services.BuildServiceProvider();

        bip39 = provider.GetRequiredService<IBip39Service>();
        bip32 = provider.GetRequiredService<IBip32Service>();
    }

    public GeneratedMnemonic GenerateFromEntropy(byte[] entropy)
    {
        var phrase = bip39.EntropyToPhrase(entropy);
        var seed = bip39.PhraseToSeed(phrase);

        return new GeneratedMnemonic(
            Convert.ToHexString(entropy).ToLowerInvariant(),
            phrase,
            Convert.ToHexString(seed).ToLowerInvariant());
    }

    public GeneratedMnemonic RecoverFromPhrase(string phrase)
    {
        var entropy = bip39.PhraseToEntropy(phrase);
        var seed = bip39.PhraseToSeed(phrase);

        return new GeneratedMnemonic(
            Convert.ToHexString(entropy).ToLowerInvariant(),
            phrase,
            Convert.ToHexString(seed).ToLowerInvariant());
    }

    public string Verify(string phrase, byte[] seed)
    {
        var derivedSeed = bip39.PhraseToSeed(phrase);
        return seed.SequenceEqual(derivedSeed) ? "OK" : "NOK";
    }

    public DerivedKeys DeriveFromSeed(byte[] seed, string? path)
    {
        var result = bip32.DeriveKeys(seed, path);

        return new DerivedKeys(
            result.ExtendedPrivateKey,
            result.ExtendedPublicKey);
    }

    public DerivedKeys DeriveFromEntropy(byte[] entropy, string? path)
    {
        var phrase = bip39.EntropyToPhrase(entropy);
        var seed = bip39.PhraseToSeed(phrase);

        return DeriveFromSeed(seed, path);
    }
}

public sealed record GeneratedMnemonic(
    string EntropyHex,
    string Phrase,
    string SeedHex);

public sealed record DerivedKeys(
    string ExtendedPrivateKey,
    string ExtendedPublicKey);

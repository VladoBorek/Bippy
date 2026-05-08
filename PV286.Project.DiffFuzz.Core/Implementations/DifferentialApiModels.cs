internal sealed record GeneratedMnemonicResult(
    string EntropyHex,
    string Phrase,
    string SeedHex);

internal sealed record DerivedKeysResult(
    string ExtendedPrivateKey,
    string ExtendedPublicKey);

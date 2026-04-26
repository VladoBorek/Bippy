using BusinessLayer.DTOs;
using BusinessLayer.Services.Interfaces;
using NBitcoin;
using ResultPattern;

namespace BusinessLayer.Services
{
    public class DeriveService : IDeriveService
    {
        public Result<DeriveDTO> Derive(byte[] seed, string? path)
        {
            ExtKey masterKey;

            try
            {
                // 1. Create BIP32 master key from seed
                masterKey = ExtKey.CreateFromSeed(seed);
            }
            catch (Exception ex)
            {
                return Result.Fail<DeriveDTO>($"Failed to create master key: {ex.Message}");
            }

            ExtKey derivedKey = masterKey;

            // 2. Apply derivation path if provided and not "m"
            if (!string.IsNullOrEmpty(path) && path != "m")
            {
                try
                {
                    var keyPath = new KeyPath(path);
                    derivedKey = masterKey.Derive(keyPath);
                }
                catch (Exception ex)
                {
                    return Result.Fail<DeriveDTO>($"Invalid derivation path: {ex.Message}");
                }
            }

            // 3. Export extended keys
            string xprv = derivedKey.ToString(Network.Main);
            string xpub = derivedKey.Neuter().ToString(Network.Main);

            return Result.Ok(new DeriveDTO
            {
                ExtendedPrivateKey = xprv,
                ExtendedPublicKey = xpub,
                Path = path ?? "m"
            });
        }
    }
}
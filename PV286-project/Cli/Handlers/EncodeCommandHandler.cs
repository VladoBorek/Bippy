using BusinessLayer.enums;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Validators;
using FluentResults;
using PV286_project.Cli.Commands;
using PV286_project.Cli.Interfaces;

namespace PV286_project.Cli.Handlers
{
    public class EncodeCommandHandler : ICommandHandler<EncodeCommandParsed>
    {
        private readonly IMnemonicService mnemonicService;

        public EncodeCommandHandler(IMnemonicService mnemonicService)
        {
            this.mnemonicService = mnemonicService;

        }

        public Result<string> Handle(EncodeCommandParsed command)
        {
            var format = command.Format;
            byte[]? entropyBytes = null;

            if (command.Entropy is not null)
            {
                var validationResult = EntropyValidator.IsValidEntropy(command.Entropy, format);
                if (validationResult.IsFailed)
                {
                    return Result.Fail(
                        string.Join(", ", validationResult.Errors.Select(e => e.Message))
                    );
                }

                entropyBytes = StringEntropyToBytes(command.Entropy, format);
            }

            var mnemonicSeedResult = mnemonicService.GetMnemonicSeed(entropyBytes, format);

            if (mnemonicSeedResult.IsFailed)
            {
                return Result.Fail(
                    string.Join(", ", mnemonicSeedResult.Errors.Select(e => e.Message))
                );
            }

            var dto = mnemonicSeedResult.Value;

            var output =
                $"Mnemonic : {dto.Mnemonic}{Environment.NewLine}" +
                $"Seed     : {BytesToString(dto.Seed, format)}";

            return Result.Ok(output);
        }

        private static string BytesToString(byte[] bytes, ValueFormat format)
        {
            return format == ValueFormat.Bin
                ? string.Concat(bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')))
                : Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static byte[] StringEntropyToBytes(string entropy, ValueFormat format)
        {
            return format == ValueFormat.Hex
                ? Convert.FromHexString(entropy)
                : Enumerable
                    .Range(0, entropy.Length / 8)
                    .Select(i => Convert.ToByte(entropy.Substring(i * 8, 8), 2))
                    .ToArray();
        }
    }
}

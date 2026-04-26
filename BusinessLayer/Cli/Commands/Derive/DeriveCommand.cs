using BusinessLayer.Cli.Utils.Enums;
using BusinessLayer.Services.Interfaces;

namespace BusinessLayer.Cli.Commands.Derive
{
    public class DeriveCommand : ICliCommand
    {
        public string CommandName => "derive";

        private readonly string? entropy;
        private readonly string? seed;
        private readonly string? path;
        private readonly ValueFormat format;
        private readonly IDeriveService deriveService;

        public DeriveCommand(
            string? entropy,
            string? seed,
            string? path,
            ValueFormat format,
            IDeriveService deriveService)
        {
            this.entropy = entropy;
            this.seed = seed;
            this.path = path?.Trim();
            this.format = format;
            this.deriveService = deriveService;
        }

        public bool Handle()
        {
            // XOR validation: entropy XOR seed
            if (entropy is null && seed is null)
            {
                Console.Error.WriteLine("Either --entropy or --seed must be provided.");
                return false;
            }

            if (entropy is not null && seed is not null)
            {
                Console.Error.WriteLine("Only one of --entropy or --seed may be provided.");
                return false;
            }

            byte[] seedBytes;

            try
            {
                var input = entropy ?? seed!;

                ValidateFormat(input, format);
                seedBytes = StringToBytes(input, format);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Invalid input value: {ex.Message}");
                return false;
            }

            var deriveRes = deriveService.Derive(seedBytes, NormalizePath(path));

            if (deriveRes.IsFailed)
            {
                Console.Error.WriteLine(deriveRes.Error);
                return false;
            }

            Console.Write(deriveRes.Value.ToString());
            return true;
        }


        private static string? NormalizePath(string? path)
        {
            return string.IsNullOrEmpty(path) || path == "m"
                ? "m"
                : path;
        }

        private static void ValidateFormat(string value, ValueFormat format)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be empty.");

            if (format == ValueFormat.Hex && value.Length % 2 != 0)
                throw new ArgumentException("Hex value must have even length.");

            if (format == ValueFormat.Bin && value.Length % 8 != 0)
                throw new ArgumentException("Binary value length must be divisible by 8.");
        }

        private static byte[] StringToBytes(string value, ValueFormat format)
        {
            return format == ValueFormat.Hex
                ? Convert.FromHexString(value)
                : Enumerable
                    .Range(0, value.Length / 8)
                    .Select(i => Convert.ToByte(value.Substring(i * 8, 8), 2))
                    .ToArray();
        }
    }
}
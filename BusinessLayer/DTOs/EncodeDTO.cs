using BusinessLayer.Cli.Utils.Enums;
using NBitcoin;

namespace BusinessLayer.DTOs
{
    public class EncodeDTO
    {
        public required Mnemonic Mnemonic { get; set; }
        public required byte[] Seed { get; set; }
        public required ValueFormat Format { get; set; }

        public override string ToString()
        {
            return $"Mnemonic : {Mnemonic}{Environment.NewLine}"
                + $"Seed     : {BytesToString(Seed, Format)}";
        }

        private static string BytesToString(byte[] bytes, ValueFormat format)
        {
            return format == ValueFormat.Bin
                ? string.Concat(bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')))
                : Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}

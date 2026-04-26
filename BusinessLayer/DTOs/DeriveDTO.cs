namespace BusinessLayer.DTOs
{
    public class DeriveDTO
    {
        public required string ExtendedPrivateKey { get; init; }
        public required string ExtendedPublicKey { get; init; }
        public required string Path { get; init; }

        public override string ToString()
        {
            return  $"XPrv   : {ExtendedPrivateKey}{Environment.NewLine}"
                   + $"XPub   : {ExtendedPublicKey}";
        }
    }
}
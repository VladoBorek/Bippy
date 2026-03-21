namespace BusinessLayer.CLI.Commands.Help
{
    public class HelpCommand : ICliCommand
    {
        public string CommandName => "help";

        public bool Handle()
        {
            Console.Write(
                @"Mnemonic CLI

                Generate a BIP-39 mnemonic phrase and seed.

                Usage:
                  dotnet run -- encode [--entropy <value> --format <hex|bin>]
                  dotnet run -- --help

                Options:
                  --entropy <value>    Optional entropy input.
                  --format <hex|bin>   Format of the provided entropy.
                  --help               Show this help message.

                Notes:
                  If '--entropy' is omitted, random entropy is generated automatically.
                  Supported entropy formats are hexadecimal and binary.

                Examples:
                  dotnet run -- encode
                  dotnet run -- encode --entropy 7ab32212dc82f67d9f38e254a6fd02c730b530f9f5e67d3c6bbdb62efb2a127 --format hex"
            );

            return true;
        }
    }
}

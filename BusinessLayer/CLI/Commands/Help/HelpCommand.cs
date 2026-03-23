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
                  dotnet run -- batch <filepath|-> [commands...]
                  dotnet run -- --help

                Options:
                  --entropy <value>    Optional entropy input.
                  --format <hex|bin>   Format of the provided entropy.
                  --help               Show this help message.

                Batch Mode:
                  batch <filepath>       Execute a batch of commands from a text file.
                  batch - ""... | ...""    Execute an inline batch of commands separated by '|'.

                Notes:
                  If '--entropy' is omitted, random entropy is generated automatically.
                  Supported entropy formats are hexadecimal and binary.

                Examples:
                  dotnet run -- encode
                  dotnet run -- encode --entropy 78ba6f96c8a70f71c4acff1c9dc7b35d8988734180d9502eeada775b7cca103e --format hex
                  dotnet run -- batch ""C:\batch.txt""
                  dotnet run -- batch - ""encode --format bin | encode"""
            );

            return true;
        }
    }
}

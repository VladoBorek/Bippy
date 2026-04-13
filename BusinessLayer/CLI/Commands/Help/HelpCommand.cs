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
                  dotnet run -- decode <mnemonic phrase> [--format <hex|bin>]
                  dotnet run -- verify --phrase <mnemonic phrase> --seed <value> [--format <hex|bin>]
                  dotnet run -- batch <filepath|->
                  dotnet run -- --help

                Options:
                  --entropy <value>    Entropy input for 'encode'.
                  --phrase <value>     Mnemonic phrase input for 'verify'.
                  --seed <value>       Seed input for 'verify'.
                  --format <hex|bin>   Input/output format where applicable.
                  --help               Show this help message.

                Batch Mode:
                  batch <filepath>     Execute commands from a text file, one per line.
                  batch - ""...|...""    Execute inline commands separated by '|'.

                Notes:
                  If '--entropy' is omitted in 'encode', random entropy is generated.
                  'decode' outputs entropy and seed for a valid mnemonic phrase.
                  'verify' outputs OK if the phrase generates the seed, otherwise NOK.

                Examples:
                  dotnet run -- encode
                  dotnet run -- encode --entropy 78ba6f96c8a70f71c4acff1c9dc7b35d8988734180d9502eeada775b7cca103e --format hex
                  dotnet run -- decode ""photo memory captain decline vendor heavy seminar gloom mouse economy awkward tilt"" --format hex
                  dotnet run -- verify --phrase ""judge square toss mule ill rib bargain paper broken until under roast obtain defy alcohol brass expand jar repair upgrade result govern domain solid"" --seed 897f9beefb28fa6660e65a6b77518547d1bf8ad203cae84cf5614174fce86d8c8329547779a319090c4557fd330b36b294a1cc9bcaaf5c3f2b48eefbe5142340 --format hex
                  dotnet run -- batch ""C:\batch.txt""
                  dotnet run -- batch - ""encode --format bin | encode"""
            );

            return true;
        }
    }
}

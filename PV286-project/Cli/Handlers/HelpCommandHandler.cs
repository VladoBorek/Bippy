using PV286_project.Cli.Commands;
using FluentResults;
using PV286_project.Cli.Interfaces;


namespace PV286_project.Cli.Handlers
{
    // # TODO implement a proper help message that lists all available commands and their usage
    // Still leaving this here until we decide on final solution how to handle logic of prining out final output and how to handle The Services 
    public class HelpCommandHandler : ICommandHandler<HelpCommandParsed>
    {
        public Result<string> Handle(HelpCommandParsed command)
        {
            return Result.Ok(
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

        }

    }
}

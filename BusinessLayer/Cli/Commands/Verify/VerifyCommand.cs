using BusinessLayer.CLI.Commands;
using BusinessLayer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Cli.Commands.Verify
{
    public class VerifyCommand : ICliCommand
    {
        public string CommandName => "verify";

        private readonly string phrase;
        private readonly byte[] seed;
        private readonly IVerifyService verifyService;

        public VerifyCommand(string phrase, byte[] seed, IVerifyService verifyService)
        {
            this.phrase = phrase;
            this.seed = seed;
            this.verifyService = verifyService;
        }

        public bool Handle()
        {
            var verifyRes = verifyService.Verify(phrase, seed);

            if (verifyRes.IsFailed)
            {
                Console.Error.WriteLine(verifyRes.Error);
                return false;
            }

            Console.Write(verifyRes.Value.ToString());
            return true;
        }
    }
}

using System;
using System.IO;
using DitoDisco.CommandLineArgs;


namespace Example {

    internal static class Program {

        static void ProcessText(StreamReader reader, Action<char> writeDelegate, double ratio, bool shouty) {

            bool inWhitespace = false;
            while(true) {
                int next = reader.Read();
                if(next == -1) break;

                char ch = (char)next;

                bool should = Random.Shared.NextDouble() < ratio;

                if(should) {
                    ch = Random.Shared.NextDouble() <= 0.5 ? char.ToUpper(ch) : char.ToLower(ch);
                }

                if(shouty && !inWhitespace && char.IsWhiteSpace(ch)) {
                    writeDelegate('!');
                }

                inWhitespace = char.IsWhiteSpace(ch);

                writeDelegate(ch);
            }

        }


        public static void Main( string[] args ) {
            var ratioOpt = new Option(ValueExpectation.Optional, "r", "ratio");
            var outOpt = new Option(ValueExpectation.Required, "o", "output");
            var shoutyOpt = new Option(ValueExpectation.NotAllowed, "s", "shouty", "loud");

            var options = new CommandLineOptions(args, new Option[] { ratioOpt, outOpt }, allowRedefinition: false);
        }

    }

}

using System;
using System.IO;
using DitoDisco.CommandLineArgs;


namespace Example {

    internal static class Program {

        /// <summary>
        /// 
        /// </summary>
        static void ProcessText(StreamReader reader, Action<char> writeDelegate, double ratio, double shoutiness, bool slow) {

            bool shouty = shoutiness > 0;
            char? lastCh = null;
            while(true) {
                int next = reader.Read();
                if(next == -1) break;

                char ch = (char)next;

                bool should = Random.Shared.NextDouble() < ratio;

                if(should) {
                    ch = Random.Shared.NextDouble() <= 0.5 ? char.ToUpper(ch) : char.ToLower(ch);
                }

                // Shout only if we're going from a letter to whitespace
                if(shouty && lastCh.HasValue && char.IsLetter(lastCh.Value) && char.IsWhiteSpace(ch)) {
                    while(Random.Shared.NextDouble() < shoutiness) {
                        writeDelegate('!');
                    }
                }

                writeDelegate(ch);
                lastCh = ch;

                if(slow) System.Threading.Thread.Sleep(100);
            }

        }


        public static void Main( string[] args ) {

            var ratioOpt = new Option(ValueExpectation.Required, "r", "ratio");
            var shoutyOpt = new Option(ValueExpectation.Optional, "s", "shouty", "loud");
            var outOpt = new Option(ValueExpectation.Required, "o", "output");
            var slowOpt = new Option(ValueExpectation.NotAllowed, "S", "slow");

            var options = new CommandLineOptions(args, new Option[] { ratioOpt, shoutyOpt, outOpt, slowOpt }, allowRedefinition: false);

            if(options.PositionalArguments.Count == 0) {
                Console.WriteLine("Usage:\n\n[-r|--ratio=] [-s|--shouty=|--loud=] [-o|--output=] [-S|--slow] FILES");
                return;
            }


            double ratio = 0.5;
            double shoutiness = 0;
            Action<char> writeDelegate = Console.Write;
            bool slow = options.Options.ContainsKey(slowOpt);

            if(options.Options.TryGetValue(ratioOpt, out string? ratioString)) ratio = double.Parse(ratioString! /* We can ! it, since it's required to have a value. */);
            if(options.Options.TryGetValue(shoutyOpt, out string? shoutyString)) { // FIXME doesn't work without a value
                if(shoutyString != null) shoutiness = double.Parse(shoutyString);
                else shoutiness = 0.5;
            }

            StreamWriter? streamWriter = null;
            if(options.Options.TryGetValue(outOpt, out string? outString)) {
                streamWriter = new StreamWriter(File.Open(outString!, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Delete));
                writeDelegate = streamWriter.Write;
            }

            foreach(string posArg in options.PositionalArguments) {
                using(var reader = new StreamReader(File.Open(posArg, FileMode.Open, FileAccess.Read, FileShare.Delete))) {
                    ProcessText(reader, writeDelegate, ratio, shoutiness, slow);
                }
            }

            streamWriter?.Dispose();

        }

    }

}

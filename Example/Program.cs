using System;
using System.IO;
using System.Collections.Generic;
using DitoDisco.CommandLineArgs;


namespace Example {

    internal static class Program {

        /// <summary>
        /// Randomizes the case of some characters, and adds exclamation! after words optionally.
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
                        if(slow) System.Threading.Thread.Sleep(100);
                    }
                }

                writeDelegate(ch);
                lastCh = ch;

                if(slow) System.Threading.Thread.Sleep(100);
            }

        }


        public static void Main( string[] args ) {

            var ratioOpt = new Option(ValueExpectation.Required, "r", "ratio"); // Chance of changing capitalization per character
            var shoutyOpt = new Option(ValueExpectation.Optional, "s", "shouty", "loud"); // Each time this probability is passed at the end of a word, an exclamation mark is added!!!
            var outOpt = new Option(ValueExpectation.Required, "o", "output"); // Name of file to write into, if unspecified, write to stdout
            var slowOpt = new Option(ValueExpectation.NotAllowed, "S", "slow"); // Whether to insert an artificial delay after writing each character

            var options = new CommandLineOptions(
                args, // Arguments passed to the program. !!! NOTE !!!: 'dotnet run' likes to mess with argument passing, so something like 'dotnet run -s -- text.txt' won't work. Run the executable directly instead.
                new Option[] { ratioOpt, shoutyOpt, outOpt, slowOpt }, // List of options we expect. CommandLineOptions needs to know this, because "-short" could be "-s hort" or "-s -h -o -r -t" depending on whether -s requires a value or not.
                allowRedefinition: false // Disallow defining the same option multiple times
            );


#if DEBUG
            // Debug only: print stuff
            Console.WriteLine("Options enabled:");
            foreach(KeyValuePair<Option, string?> kvp in options.Options) {
                Console.Write($"-{kvp.Key.names[0]}");
                if(kvp.Value != null) Console.Write($" = {kvp.Value}");

                Console.WriteLine();
            }

            Console.WriteLine("Positional arguments:");
            foreach(string posArg in options.PositionalArguments) {
                Console.WriteLine(posArg);
            }
#endif


            // No input files? Print usage.
            if(options.PositionalArguments.Count == 0) {
                Console.WriteLine("Usage:\n[-r|--ratio=] [-s|--shouty=|--loud=] [-o|--output=] [-S|--slow] FILES");
                return;
            }


            double ratio = 0.5;
            double shoutiness = 0;
            Action<char> writeDelegate = Console.Write;
            bool slow = options.Options.ContainsKey(slowOpt); // Check for the presence of the slow flag. It accepts no value, so we just do this directly.

            // Ratio option
            if(options.Options.TryGetValue(ratioOpt, out string? ratioString)) ratio = double.Parse(ratioString! /* We may use the damn-it operator here, since ratioOpt required to have a value. */);

            // Shouty option
            if(options.Options.TryGetValue(shoutyOpt, out string? shoutyString)) {
                if(shoutyString != null) shoutiness = double.Parse(shoutyString);
                else shoutiness = 0.5; // Default value if it's just specified on its own
            }

            // If we are writing to a file, open it
            StreamWriter? streamWriter = null;
            if(options.Options.TryGetValue(outOpt, out string? outString)) {
                streamWriter = new StreamWriter(File.Open(outString!, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Delete));
                writeDelegate = streamWriter.Write;
            }

            // Every positional argument is an input file name
            foreach(string posArg in options.PositionalArguments) {
                using(var reader = new StreamReader(File.Open(posArg, FileMode.Open, FileAccess.Read, FileShare.Delete))) {
                    ProcessText(reader, writeDelegate, ratio, shoutiness, slow);
                }
            }

            // Close the file we might've opened
            streamWriter?.Dispose();

        }

    }

}

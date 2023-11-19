using System;


namespace DitoDisco.CommandLineArgs {

    /// <summary>
    /// Thrown when parsing command line arguments fails for some reason, due to incorrect user input.
    /// </summary>
    public sealed class CommandLineParseException : Exception {

        private readonly string _message;
        public override string Message => _message;


        public CommandLineParseException(string message = "Failed to parse command line arguments for some reason.") {
            _message = message;
        }

    }

}

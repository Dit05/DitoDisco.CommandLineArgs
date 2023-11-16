using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;


namespace DitoDisco.CommandLineArgs {

    /// <summary>
    /// Represents parsed command line arguments organized as "options" with optional values, and "positional arguments".
    /// This type is immutable.
    /// </summary>
    public sealed class CommandLineOptions {

        /// <returns>Whether <paramref name="rune"/> is allowed in the name of an option.</returns>
        public static bool IsRuneAllowedInOptionName(Rune rune) => Rune.IsLetter(rune) || Rune.IsDigit(rune) || (rune.IsAscii && "-_".Contains((char)rune.Value));

        static string RuneToReadableString(Rune rune) {
            if(Rune.IsControl(rune) || Rune.IsWhiteSpace(rune)) {
                return $"(Unicode scalar {rune.Value})";
            } else {
                return $"'{rune}'";
            }
        }

        [DoesNotReturn]
        static void ThrowInvalidRuneInOption(Rune rune) {
            throw new CommandLineParseException($"Character not allowed in option name: {RuneToReadableString(rune)}");
        }

        [DoesNotReturn]
        static void ThrowUnrecognizedOption(string name) {
            string prefix = name.Length > 1 ? LongOptionPrefix : OptionPrefix.ToString();
            throw new CommandLineParseException($"Unrecognized option: '{prefix}{name}'.");
        }


        public static readonly string OptionListTerminator = "--";
        public static readonly char OptionPrefix = '-';
        public static readonly string LongOptionPrefix = "--";
        private static readonly Rune EqualsRune = new Rune('=');


        //


        readonly ImmutableDictionary<Option, string?> options;
        /// <summary>Dictionary of options present and their values. Their value is null if they don't accept one or they optionally accept one and no value is present.</summary>
        public IReadOnlyDictionary<Option, string?> Options => options;

        readonly ImmutableArray<string> positionalArguments;
        /// <summary>Array of positional arguments. Every argument that is not an option or the value of an option or the first "--" is a positional argument.</summary>
        public IReadOnlyList<string> PositionalArguments => positionalArguments;


        /// <summary>
        /// Creates a new set of parsed command line arguments, expecting a limited subset of options.
        /// </summary>
        /// <param name="allowedOptions">List of allowed options. Any option that is not present on this list is unexpected, and throws an exception.</param>
        /// <param name="args">Arguments, like the array passed to Program.Main. Each entry should be one group of short options, a long option, or a positional argument. Your shell probably handles this for you.</param>
        /// <param name="allowRedefinition">Whether to allow the same option to be defined multiple times. When allowed, the final value will be the one last assigned.</param>
        public CommandLineOptions(IEnumerable<string> args, IEnumerable<Option> allowedOptions, bool allowRedefinition = true) {

            // Register the names of options
            var namesToOpts = new Dictionary<string, Option>();
            foreach(Option opt in allowedOptions) {
                foreach(string name in opt.names) {
                    foreach(Rune rune in name.EnumerateRunes()) {
                        if(IsRuneAllowedInOptionName(rune) == false) ThrowInvalidRuneInOption(rune);
                    }

                    if(!namesToOpts.TryAdd(name, opt)) throw new ArgumentException($"The name '{name}' is already used by another option.");
                }
            }


            var optValues = new Dictionary<Option, string?>();
            var posArgs = new List<string>();

            Option? shortOptToAssign = null;
            string? shortOptName = null; // Used name of the short option to assign; For exception message purposes.

            bool is_shortopt_value_required() => shortOptToAssign != null && shortOptToAssign.valueExpectation == ValueExpectation.Required;


            string concat_runes(ReadOnlySpan<Rune> runes) {
                var sb = new StringBuilder(capacity: runes.Length, maxCapacity: runes.Length * 2);

                Span<char> utf16 = stackalloc char[2];
                for(int i = 0; i < runes.Length; i++) {
                    int count = runes[i].EncodeToUtf16(utf16);

                    sb.Append(utf16[0]);
                    if(count == 2) sb.Append(utf16[1]);
                }

                return sb.ToString();
            }

            void assign_option(Option option, string? val, string? nameUsed = null /* For use in the exception message. */) {
                if(allowRedefinition) {
                    optValues[option] = val;
                } else {
                    if(!optValues.TryAdd(option, val)) throw new CommandLineParseException(nameUsed != null ? $"Duplicate option '{(nameUsed.Length > 1 ? LongOptionPrefix : OptionPrefix)}{nameUsed}'." : "Duplicate option.");
                }
            }

            // -abcd [value (the next argument, potentially)]
            void parse_short(ReadOnlySpan<Rune> arg /* Guaranteed to be at least 2 chars long at call site. */) {
                arg = arg.Slice(start: 1); // Skip the option prefix

                string firstName = arg[0].ToString();
                Option? firstOpt;

                if(!namesToOpts.TryGetValue(firstName, out firstOpt)) {
                    ThrowUnrecognizedOption(firstName);
                }

                if(arg.Length == 1) {
                    // Value is next argument, if option is allowed to have a value
                    if(firstOpt.valueExpectation != ValueExpectation.NotAllowed) {
                        shortOptToAssign = firstOpt;
                        shortOptName = firstName;
                    } else {
                        assign_option(firstOpt, null, firstName);
                    }
                } else {
                    bool isMultiOption = (firstOpt.valueExpectation == ValueExpectation.NotAllowed);
                    ReadOnlySpan<Rune> restOfArg = arg.Slice(start: 1);

                    if(isMultiOption) {
                        // Several valueless options in one
                        assign_option(firstOpt, null, firstName);

                        for(int i = 0; i < restOfArg.Length; i++) {
                            string optName = restOfArg[i].ToString();

                            Option? opt;
                            if(!namesToOpts.TryGetValue(optName, out opt)) ThrowUnrecognizedOption(optName);
                            if(opt.valueExpectation == ValueExpectation.Required) throw new CommandLineParseException($"Option '{OptionPrefix}{optName}' requires a value, so it cannot be part of a short option group.");

                            assign_option(opt, null, optName);
                        }
                    } else {
                        // The value is the rest of the argument since the option can have a value
                        assign_option(firstOpt, concat_runes(restOfArg), firstName);
                    }
                }
            }

            // --key[=value]
            void parse_long(ReadOnlySpan<Rune> arg) {
                string? optionName = null;
                string? optionValue = null;

                arg = arg.Slice(start: LongOptionPrefix.Length);

                for(int i = 0; i < arg.Length; i++) {
                    Rune rune = arg[i];

                    if(rune.IsAscii && (char)rune.Value == '=') {
                        // Has value
                        optionName = concat_runes(arg.Slice(start: 0, length: i));
                        optionValue = concat_runes(arg.Slice(start: i + 1));
                        break;
                    }
                }

                optionName ??= concat_runes(arg);
                Option? opt;
                if(!namesToOpts.TryGetValue(optionName, out opt)) ThrowUnrecognizedOption(optionName);

                if(optionValue == null && opt.valueExpectation == ValueExpectation.Required) throw new CommandLineParseException($"Option '{LongOptionPrefix}{optionName}' requires a value.");
                if(optionValue != null && opt.valueExpectation == ValueExpectation.NotAllowed) throw new CommandLineParseException($"Option '{LongOptionPrefix}{optionName}' cannot have a value.");

                assign_option(opt, optionValue, optionName);
            }

            // Parse each argument
            IEnumerator<string> enumerator = args.GetEnumerator();
            while(enumerator.MoveNext()) {
                string arg = enumerator.Current;

                if(arg == OptionListTerminator) {
                    if(shortOptToAssign != null) {
                        if(shortOptToAssign.valueExpectation == ValueExpectation.Required) throw new CommandLineParseException($"{OptionPrefix}{shortOptName}: Expected value, found option list terminator.");
                        else assign_option(shortOptToAssign, null, shortOptName);
                    }

                    while(enumerator.MoveNext()) {
                        posArgs.Add(enumerator.Current);
                    }
                    break;
                }


                ReadOnlySpan<Rune> make_runes_span(bool stopOnEquals) {
                    var list = new List<Rune>(arg.EnumerateRunes());

                    // Make sure they're allowed
                    if(stopOnEquals) {
                        foreach(Rune rune in list) {
                            if(rune == EqualsRune) break; // Stop checking
                            else if(IsRuneAllowedInOptionName(rune) == false) ThrowInvalidRuneInOption(rune);
                        }
                    } else {
                        foreach(Rune rune in list) {
                            if(IsRuneAllowedInOptionName(rune) == false) ThrowInvalidRuneInOption(rune);
                        }
                    }

                    return list.ToArray();
                }


                if(arg.StartsWith(LongOptionPrefix)) {
                    if(is_shortopt_value_required()) throw new CommandLineParseException($"{OptionPrefix}{shortOptName}: Expected value, found a long option.");
                    parse_long(make_runes_span(stopOnEquals: true));
                } else if(arg.StartsWith(OptionPrefix) && arg.Length > 1 /* "-" counts as a positional argument */) {
                    if(is_shortopt_value_required()) throw new CommandLineParseException($"{OptionPrefix}{shortOptName}: Expected value, found an option.");
                    parse_short(make_runes_span(stopOnEquals: false));
                } else {
                    if(shortOptToAssign != null && shortOptToAssign.valueExpectation != ValueExpectation.NotAllowed) {
                        assign_option(shortOptToAssign, arg, shortOptName);
                        shortOptToAssign = null;
                        shortOptName = null;
                    } else {
                        posArgs.Add(arg);
                    }
                }
            }

            if(is_shortopt_value_required()) throw new CommandLineParseException($"{OptionPrefix}{shortOptName}: Expected value.");

            options = ImmutableDictionary.CreateRange<Option, string?>(optValues);
            positionalArguments = ImmutableArray.CreateRange<string>(posArgs);
        }

    }

}

using System.Collections.Generic;


namespace DitoDisco.CommandLineArgs {

    /// <summary>
    /// An option to be accepted by <see cref="CommandLineOptions"/>.
    /// </summary>
    public sealed class Option {

        /// <summary>Whether this options expects, allows, or forbids a value.</summary>
        public ValueExpectation valueExpectation;
        /// <summary>Names this option is referred to by. If it's a single rune (one non-surrogate char, or a surrogate pair), then it's a short option. Otherwise, it's a long one.</summary>
        public readonly IList<string> names;


        public Option(ValueExpectation valueExpectation, params string[] names) {
            this.valueExpectation = valueExpectation;
            this.names = new List<string>(names);
        }

    }

}

using System.Collections.Generic;


namespace DitoDisco.CommandLineArgs {

    public sealed class Option {

        public ValueExpectation valueExpectation;
        public readonly IList<string> names;


        public Option(ValueExpectation valueExpectation, params string[] names) {
            this.valueExpectation = valueExpectation;
            this.names = new List<string>(names);
        }

    }

}

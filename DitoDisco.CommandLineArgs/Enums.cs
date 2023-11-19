

namespace DitoDisco.CommandLineArgs {

    /// <summary>
    /// Describes whether an <see cref="Option"/> can have- or requires a value.
    /// </summary>
    public enum ValueExpectation {
        /// <summary>This option may have a value, but it's not required.</summary>
        Optional = 0,

        /// <summary>This option must not have a value.</summary>
        NotAllowed,

        /// <summary>This option must have a value.</summary>
        Required
    }

}

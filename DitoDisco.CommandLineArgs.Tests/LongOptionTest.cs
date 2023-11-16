namespace DitoDisco.CommandLineArgs.Tests {

    [TestFixture]
    [TestOf(typeof(CommandLineOptions))]
    public class LongOptionTest {

        Option[] opts;

        [SetUp]
        public void Setup() {
            opts = new Option[] {
                new Option(ValueExpectation.Required, "r", "required"),
                new Option(ValueExpectation.Optional, "o", "optional"),
                new Option(ValueExpectation.NotAllowed, "n", "notallowed"),
            };
        }

        [Test]
        public void NothingTest() {
            _ = new CommandLineOptions(Array.Empty<string>(), opts, allowRedefinition: false);
        }

        [Test]
        public void CorrectTest() {
            var clOpts = new CommandLineOptions(new string[] { "--required=15", "--optional", "PAH", "--notallowed", "whap", "--", "--required" }, opts, allowRedefinition: false);

            Assert.That(clOpts.PositionalArguments.Count == 3);
            Assert.That(clOpts.PositionalArguments[0] == "PAH");
            Assert.That(clOpts.PositionalArguments[1] == "whap");
            Assert.That(clOpts.PositionalArguments[2] == "--required");

            Assert.That(clOpts.Options.ContainsKey(opts[0]));
            Assert.That(clOpts.Options.ContainsKey(opts[1]));
            Assert.That(clOpts.Options.ContainsKey(opts[2]));

            Assert.That(clOpts.Options[opts[0]] == "15");
            Assert.That(clOpts.Options[opts[1]] == null);
            Assert.That(clOpts.Options[opts[2]] == null);
        }

    }
}

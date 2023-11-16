namespace DitoDisco.CommandLineArgs.Tests {

    [TestFixture]
    [TestOf(typeof(CommandLineOptions))]
    public class OptionTerminatorTest {

        Option[] opts;

        [SetUp]
        public void Setup() {
            opts = new Option[] {
                new Option(ValueExpectation.Optional, "a", "first"),
                new Option(ValueExpectation.NotAllowed, "b", "second"),
                new Option(ValueExpectation.Optional, "c", "third"),
                new Option(ValueExpectation.Required, "r", "required"),
            };
        }

        [Test]
        public void CorrectTest() {
            var args = new string[] { "--first", "-b", CommandLineOptions.OptionListTerminator, "-c", "--third", "foo battle" };

            var clOpts = new CommandLineOptions(args, opts, allowRedefinition: false);

            Assert.That(clOpts.PositionalArguments.Count == 3);
            Assert.That(clOpts.PositionalArguments[0] == "-c");
            Assert.That(clOpts.PositionalArguments[1] == "--third");
            Assert.That(clOpts.PositionalArguments[2] == "foo battle");

            Assert.That(clOpts.Options.ContainsKey(opts[0]));
            Assert.That(clOpts.Options.ContainsKey(opts[1]));
            Assert.That(clOpts.Options.ContainsKey(opts[2]) == false);

            Assert.That(clOpts.Options[opts[0]] == null);
            Assert.That(clOpts.Options[opts[1]] == null);
        }

        [Test]
        public void ValueIsTerminatorTest() {
            var args = new string[] { "-a", CommandLineOptions.OptionListTerminator, "--third" };

            var clOpts = new CommandLineOptions(args, opts);

            Assert.That(clOpts.Options.ContainsKey(opts[0]));
            Assert.That(clOpts.Options.ContainsKey(opts[2]) == false);
            Assert.That(clOpts.Options[opts[0]] == null);

            Assert.That(clOpts.PositionalArguments.Count == 1);
            Assert.That(clOpts.PositionalArguments[0] == args[2]);
        }

        [Test]
        public void TerminatorInsteadOfRequiredTest() {
            var args = new string[] { "-r", CommandLineOptions.OptionListTerminator, "--third" };

            try {
                var clOpts = new CommandLineOptions(args, opts, allowRedefinition: false);
            } catch(CommandLineParseException) {
                Assert.Pass();
            }

            Assert.Fail();
        }

    }

}

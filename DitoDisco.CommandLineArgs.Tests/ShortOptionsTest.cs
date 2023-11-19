namespace DitoDisco.CommandLineArgs.Tests {

    [TestFixture]
    [TestOf(typeof(CommandLineOptions))]
    public class ShortOptionsTest {

        Option[] opts;

        [SetUp]
        public void Setup() {
            opts = new Option[] {
                new Option(ValueExpectation.Required, "r"),
                new Option(ValueExpectation.Optional, "o"),
                new Option(ValueExpectation.NotAllowed, "n"),
            };
        }

        [Test]
        public void CorrectTest() {
            var args = new string[] { "-r", "Government mandated option value", "-n", "-o", "Optional ...", "-n" };

            var clOpts = new CommandLineOptions(args, opts, allowRedefinition: true);

            Assert.IsEmpty(clOpts.PositionalArguments);

            Assert.That(clOpts.Options.ContainsKey(opts[0]));
            Assert.That(clOpts.Options[opts[0]] == args[1]);

            Assert.That(clOpts.Options.ContainsKey(opts[1]));
            Assert.That(clOpts.Options[opts[1]] == args[4]);

            Assert.That(clOpts.Options.ContainsKey(opts[2]));
            Assert.That(clOpts.Options[opts[2]] == null);
        }

        [Test]
        public void UnallowedRedefinitionTest() {
            try {
                var clOpts = new CommandLineOptions(new string[] { "-n", "-n" }, opts, allowRedefinition: false);
            } catch(CommandLineParseException) {
                Assert.Pass();
            }

            Assert.Fail("Construction shouldn't've succeeded.");
        }

        [Test]
        public void RequiredValueMissingTest() {
            try {
                var clOpts = new CommandLineOptions(new string[] { "-r", "-o" }, opts);
            } catch(CommandLineParseException) {
                Assert.Pass();
            }

            Assert.Fail("Construction shouldn't've succeeded.");
        }

        [Test]
        public void RequiredOptionByItselfTest() {
            try {
                var clOpts = new CommandLineOptions(new string[] { "-r" }, opts);
            } catch(CommandLineParseException) {
                Assert.Pass();
            }

            Assert.Fail("Construction shouldn't've succeeded.");
        }

        [Test]
        public void MultiOptionTest() {
            var args = new string[] { "-no", "not a value!" };

            var clOpts = new CommandLineOptions(args, opts);

            Assert.That(clOpts.Options.Count, Is.EqualTo(2));
            Assert.That(clOpts.PositionalArguments.Count, Is.EqualTo(1));

            Assert.That(clOpts.Options.ContainsKey(opts[1]));
            Assert.That(clOpts.Options.ContainsKey(opts[2]));

            Assert.That(clOpts.PositionalArguments[0] == args[1]);

            Assert.That(clOpts.Options[opts[1]] == null);
            Assert.That(clOpts.Options[opts[2]] == null);
        }

        [Test]
        public void CrampedValueTest() {
            var args = new string[] { "-ron", "not a value!" };

            var clOpts = new CommandLineOptions(args, opts);

            Assert.That(clOpts.Options.Count, Is.EqualTo(1));
            Assert.That(clOpts.PositionalArguments.Count, Is.EqualTo(1));

            Assert.That(clOpts.Options.ContainsKey(opts[0]));

            Assert.That(clOpts.PositionalArguments[0] == args[1]);

            Assert.That(clOpts.Options[opts[0]] == "on");
        }

        [Test]
        public void FauxLongOptionTest() {
            var args = new string[] { "-required=yes" /* note how it starts with r to throw the constructor off and make it think it's opts[0] */ };

            try {
                var clOpts = new CommandLineOptions(args, opts);
            } catch(CommandLineParseException) {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void OptionalAtEndTest() {
            var optional = new Option(ValueExpectation.Optional, "o");

            var args = new string[] { "-o" };

            var clOpts = new CommandLineOptions(args, new Option[] { optional });

            Assert.That(clOpts.PositionalArguments, Is.Empty);
            Assert.That(clOpts.Options.ContainsKey(optional));
            Assert.That(clOpts.Options[optional], Is.Null);
        }

    }
}

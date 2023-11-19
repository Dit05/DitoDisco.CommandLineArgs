# DitoDisco.CommandLineArgs
Parses arguments passed to the executable and lets you retrieve their values easily.

Implements the syntax described at https://www.gnu.org/software/libc/manual/html_node/Argument-Syntax.html.

## Usage
- Define your options:
  - Specify what name(s) they're called
  - Specify whether they can have a value or behave like flags
- Create a `CommandLineOptions` using an array of accepted options and an array of arguments to parse.
- In the created object, find:
  - Which of your options are defined
  - Whether they've been given values
  - Positional (non-option) arguments

See also [the example project](./Example/Program.cs).


## Contributing
If you've found a bug and there's no open issue about it, open one yourself. It helps if you also write a failing unit test for it.

If you'd like to just help fix existing bugs instead, help investiage open issues or create a pull request with your fix.  
Pull requests should be limited to fixes, and should conform to the project's code style.

using System.CommandLine;

namespace SystemCommandLine.Extensions;
/// <summary>
/// Represents a method that maps parsed command-line arguments to an options object.
/// </summary>
/// <remarks>Implementations of this delegate are commonly used to transfer values from a <see
/// cref="ParseResult"/> to a strongly-typed options instance in command-line parsing scenarios. The delegate does not
/// return a value; it is expected to modify <paramref name="options"/> directly.</remarks>
/// <param name="parseResult">The result of parsing the command-line arguments. Provides access to the values and tokens parsed from user input.</param>
/// <param name="options">The options object to be populated or configured based on the parsed arguments. Typically, properties of this object
/// are set according to the values in <paramref name="parseResult"/>.</param>
public delegate void ArgumentMapper(ParseResult parseResult, object options);
/// <summary>
/// Represents a method that registers an argument mapper for use in argument mapping operations.
/// </summary>
/// <param name="mapper">The argument mapper to be registered. Cannot be null.</param>
public delegate void ArgumentMapperRegistration(ArgumentMapper mapper);

internal class CommandArgumentMapper<TCommand> : List<ArgumentMapper> { }

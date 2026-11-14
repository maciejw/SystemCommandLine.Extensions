using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>Copies a parsed command-line value into an options instance.</summary>
/// <param name="parseResult">The parse result from which to read the value.</param>
/// <param name="options">The options instance to populate, of the type expected by this mapper.</param>
public delegate void ArgumentMapper(ParseResult parseResult, object options);

/// <summary>Registers a mapper to run when the command's bound options are configured.</summary>
/// <param name="mapper">The mapping operation to register.</param>
public delegate void ArgumentMapperRegistration(ArgumentMapper mapper);

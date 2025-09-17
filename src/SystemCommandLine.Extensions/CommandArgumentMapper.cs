using System.CommandLine;

namespace SystemCommandLine.Extensions;

public delegate void ArgumentMapper(ParseResult parseResult, object options);
public delegate void ArgumentMapperRegistration(ArgumentMapper mapper);

internal class CommandArgumentMapper<TCommand> : List<ArgumentMapper> { }

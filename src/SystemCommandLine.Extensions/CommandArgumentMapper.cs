using System.CommandLine;

namespace SystemCommandLine.Extensions;

public delegate void OptionMapper(ParseResult parseResult, object options);
public delegate void ArgumentMapperRegistration(OptionMapper mapper);

public class CommandArgumentMapper<TCommand> : List<OptionMapper> { }

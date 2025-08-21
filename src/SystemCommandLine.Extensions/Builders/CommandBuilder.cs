using System.CommandLine;

namespace SystemCommandLine.Extensions.Builders;

public class CommandBuilder<TCommand>(TCommand command, ArgumentMapperRegistration? mapperRegistration)
    where TCommand : Command, IUseCommandBuilder<TCommand>
{
    public CommandArgumentBuilder<TCommand, TOptionHolder> With<TOptionHolder>()
        where TOptionHolder : class
    {
        return new CommandArgumentBuilder<TCommand, TOptionHolder>(command, mapperRegistration);
    }
}

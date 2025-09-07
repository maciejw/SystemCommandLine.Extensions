using System.CommandLine;

namespace SystemCommandLine.Extensions.Builders;

public class CommandBuilder<TCommand>(TCommand command)
    where TCommand : Command, IUseCommandBuilder<TCommand>
{
    public CommandArgumentBuilder<TCommand, TOption> NewOption<TOption>(string name)
    {
        return new CommandArgumentBuilder<TCommand, TOption>(this, command, name);
    }

    public CommandBuilderWithMapping<TCommand, TOptionHolder> WithMapping<TOptionHolder>(ArgumentMapperRegistration mapperRegistration)
        where TOptionHolder : class
    {
        return new CommandBuilderWithMapping<TCommand, TOptionHolder>(this, command, mapperRegistration);
    }
}

using System.CommandLine;

namespace SystemCommandLine.Extensions.Builders;

internal class CommandBuilder<TCommand>(TCommand command) : ICommandBuilder<TCommand> where TCommand : Command, ICommandDefinition<TCommand>
{
    public ICommandPositionalArgumentBuilder<TCommand, TValue> NewArgument<TValue>(string name)
        => new CommandPositionalArgumentBuilder<TCommand, TValue>(this, command, name);

    public ICommandOptionBuilder<TCommand, TValue> NewOption<TValue>(string name)
    {
        return new CommandOptionBuilder<TCommand, TValue>(this, command, name);
    }

    public ICommandBuilderWithMapping<TCommand, TOptions> WithMapping<TOptions>(SymbolMapperRegistration mapperRegistration)
        where TOptions : class
    {
        return new CommandBuilderWithMapping<TCommand, TOptions>(this, command, mapperRegistration);
    }
}

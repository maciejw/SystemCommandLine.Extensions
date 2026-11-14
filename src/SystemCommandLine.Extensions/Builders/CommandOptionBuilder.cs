using System.CommandLine;

namespace SystemCommandLine.Extensions.Builders;

internal class CommandOptionBuilder<TCommand, TValue>(ICommandBuilder<TCommand> commandBuilder, TCommand command, string name) : ICommandOptionBuilder<TCommand, TValue> where TCommand : Command, ICommandDefinition<TCommand>
{
    protected readonly Option<TValue> option = new(NameFormatExtensions.ToKebabCase("--", name));

    public virtual ICommandOptionBuilder<TCommand, TValue> Configure(Action<Option<TValue>> configure)
    {
        configure.Invoke(option);
        return this;
    }

    public virtual ICommandBuilder<TCommand> AddToCommand()
    {
        command.Add(option);
        return commandBuilder;
    }
}

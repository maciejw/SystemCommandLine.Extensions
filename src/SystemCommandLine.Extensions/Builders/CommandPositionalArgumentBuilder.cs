using System.CommandLine;

namespace SystemCommandLine.Extensions.Builders;

internal sealed class CommandPositionalArgumentBuilder<TCommand, TValue>(
    ICommandBuilder<TCommand> builder, TCommand command, string name)
    : ICommandPositionalArgumentBuilder<TCommand, TValue>
    where TCommand : Command, IUseCommandBuilder<TCommand>
{
    private readonly Argument<TValue> argument = new(NameFormatExtensions.ToKebabCase("", name));

    public ICommandPositionalArgumentBuilder<TCommand, TValue> Configure(Action<Argument<TValue>> configure)
    {
        configure(argument);
        return this;
    }

    public ICommandBuilder<TCommand> AddToCommand()
    {
        command.Add(argument);
        return builder;
    }
}


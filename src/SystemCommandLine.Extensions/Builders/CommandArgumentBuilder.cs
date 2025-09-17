using System.CommandLine;

namespace SystemCommandLine.Extensions.Builders;

internal class CommandArgumentBuilder<TCommand, TOption>(ICommandBuilder<TCommand> commandBuilder, TCommand command, string name) : ICommandArgumentBuilder<TCommand, TOption> where TCommand : Command, IUseCommandBuilder<TCommand>
{
    protected readonly Option<TOption> option = new(NameFormatExtensions.ToKebabCase("--", name));

    public virtual ICommandArgumentBuilder<TCommand, TOption> Configure(Action<Option<TOption>> value)
    {
        value.Invoke(option);
        return this;
    }

    public virtual ICommandBuilder<TCommand> AddToCommand()
    {
        command.Add(option);
        return commandBuilder;
    }
}

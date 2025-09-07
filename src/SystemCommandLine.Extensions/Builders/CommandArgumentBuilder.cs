using System.CommandLine;

namespace SystemCommandLine.Extensions.Builders;

public class CommandArgumentBuilder<TCommand, TOption>(CommandBuilder<TCommand> commandBuilder, TCommand command, string name)
    where TCommand : Command, IUseCommandBuilder<TCommand>
{
    protected readonly Option<TOption> option = new(NameFormatExtensions.ToKebabCase("--", name));

    public virtual CommandArgumentBuilder<TCommand, TOption> Configure(Action<Option<TOption>> value)
    {
        value.Invoke(option);
        return this;
    }

    public virtual CommandBuilder<TCommand> AddToCommand()
    {
        command.Add(option);

        return commandBuilder;
    }
}

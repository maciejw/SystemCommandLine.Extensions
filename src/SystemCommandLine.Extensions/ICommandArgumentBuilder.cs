using System.CommandLine;

namespace SystemCommandLine.Extensions;

public interface ICommandArgumentBuilder<TCommand, TOption> where TCommand : Command, IUseCommandBuilder<TCommand>
{
    ICommandBuilder<TCommand> AddToCommand();
    ICommandArgumentBuilder<TCommand, TOption> Configure(Action<Option<TOption>> value);
}
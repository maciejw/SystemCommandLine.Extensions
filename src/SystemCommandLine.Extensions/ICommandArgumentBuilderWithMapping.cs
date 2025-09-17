using System.CommandLine;

namespace SystemCommandLine.Extensions;

public interface ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption>
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    ICommandBuilderWithMapping<TCommand, TOptionHolder> AddToCommand();
    ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> Configure(Action<Option<TOption>> value);
}
using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions;

public interface ICommandBuilderWithMapping<TCommand, TOptionHolder>
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    ICommandBuilder<TCommand> CommandBuilder();
    ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> NewOption<TOption>(Expression<Func<TOptionHolder, TOption>> propertyExpression);
}
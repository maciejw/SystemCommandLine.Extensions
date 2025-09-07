using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

public class CommandBuilderWithMapping<TCommand, TOptionHolder>(CommandBuilder<TCommand> commandBuilder, TCommand command, ArgumentMapperRegistration? mapperRegistration)
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    public CommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> NewOption<TOption>(Expression<Func<TOptionHolder, TOption>> propertyExpression)
    {
        return new CommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption>(command, this, propertyExpression, mapperRegistration);
    }
    public CommandBuilder<TCommand> CommandBuilder()
    {

        return commandBuilder;
    }
}
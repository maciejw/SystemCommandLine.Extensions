using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

internal class CommandBuilderWithMapping<TCommand, TOptionHolder>(ICommandBuilder<TCommand> commandBuilder, TCommand command, ArgumentMapperRegistration mapperRegistration) : ICommandBuilderWithMapping<TCommand, TOptionHolder> where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    public ICommandPositionalArgumentBuilderWithMapping<TCommand, TOptionHolder, TValue> NewArgument<TValue>(Expression<Func<TOptionHolder, TValue>> propertyExpression)
        => new CommandPositionalArgumentBuilderWithMapping<TCommand, TOptionHolder, TValue>(this, command, propertyExpression, mapperRegistration);

    public ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> NewOption<TOption>(Expression<Func<TOptionHolder, TOption>> propertyExpression)
    {
        return new CommandOptionBuilderWithMapping<TCommand, TOptionHolder, TOption>(command, this, propertyExpression, mapperRegistration);
    }
    public ICommandBuilder<TCommand> CommandBuilder() => commandBuilder;
}

using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

internal class CommandBuilderWithMapping<TCommand, TOptions>(ICommandBuilder<TCommand> commandBuilder, TCommand command, SymbolMapperRegistration mapperRegistration) : ICommandBuilderWithMapping<TCommand, TOptions> where TCommand : Command, ICommandDefinition<TCommand>
    where TOptions : class
{
    public ICommandPositionalArgumentBuilderWithMapping<TCommand, TOptions, TValue> NewArgument<TValue>(Expression<Func<TOptions, TValue>> propertyExpression)
        => new CommandPositionalArgumentBuilderWithMapping<TCommand, TOptions, TValue>(this, command, propertyExpression, mapperRegistration);

    public ICommandOptionBuilderWithMapping<TCommand, TOptions, TValue> NewOption<TValue>(Expression<Func<TOptions, TValue>> propertyExpression)
    {
        return new CommandOptionBuilderWithMapping<TCommand, TOptions, TValue>(command, this, propertyExpression, mapperRegistration);
    }
    public ICommandBuilder<TCommand> CommandBuilder() => commandBuilder;
}

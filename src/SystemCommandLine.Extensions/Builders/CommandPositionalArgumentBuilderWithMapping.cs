using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

internal sealed class CommandPositionalArgumentBuilderWithMapping<TCommand, TOptions, TValue>(
    ICommandBuilderWithMapping<TCommand, TOptions> builder, TCommand command, Expression<Func<TOptions, TValue>> propertyExpression, SymbolMapperRegistration mapperRegistration)
    : ICommandPositionalArgumentBuilderWithMapping<TCommand, TOptions, TValue>
    where TCommand : Command, ICommandDefinition<TCommand>
    where TOptions : class
{
    private readonly Argument<TValue> argument = new(NameFormatExtensions.ToKebabCase("", propertyExpression.GetPropertyName()));

    public ICommandPositionalArgumentBuilderWithMapping<TCommand, TOptions, TValue> Configure(Action<Argument<TValue>> configure)
    {
        configure(argument);
        return this;
    }

    public ICommandBuilderWithMapping<TCommand, TOptions> AddToCommand()
    {
        var assign = propertyExpression.CreatePropertyValueMapper();
        command.Add(argument);
        mapperRegistration((parseResult, options) =>
        {
            if (options is TOptions holder)
                assign(holder, parseResult.GetValue(argument));
        });
        return builder;
    }
}


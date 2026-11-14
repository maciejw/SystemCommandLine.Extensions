using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

internal sealed class CommandPositionalArgumentBuilderWithMapping<TCommand, TOptionHolder, TValue>(
    ICommandBuilderWithMapping<TCommand, TOptionHolder> builder, TCommand command, Expression<Func<TOptionHolder, TValue>> propertyExpression, ArgumentMapperRegistration mapperRegistration)
    : ICommandPositionalArgumentBuilderWithMapping<TCommand, TOptionHolder, TValue>
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    private readonly Argument<TValue> argument = new(NameFormatExtensions.ToKebabCase("", propertyExpression.GetPropertyName()));

    public ICommandPositionalArgumentBuilderWithMapping<TCommand, TOptionHolder, TValue> Configure(Action<Argument<TValue>> configure)
    {
        configure(argument);
        return this;
    }

    public ICommandBuilderWithMapping<TCommand, TOptionHolder> AddToCommand()
    {
        var assign = propertyExpression.CreateArgumentValueMapper();
        command.Add(argument);
        mapperRegistration((parseResult, options) =>
        {
            if (options is TOptionHolder holder)
                assign(holder, parseResult.GetValue(argument));
        });
        return builder;
    }
}


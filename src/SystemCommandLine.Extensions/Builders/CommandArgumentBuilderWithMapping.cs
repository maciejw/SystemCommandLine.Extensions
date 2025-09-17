using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

internal class CommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption>(TCommand command, ICommandBuilderWithMapping<TCommand, TOptionHolder> commandHandlerBuilder, Expression<Func<TOptionHolder, TOption>> propertyExpression, ArgumentMapperRegistration mapperRegistration) : ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    private readonly Option<TOption> option = new(ToKebabCase(propertyExpression.GetPropertyName()));

    private static string ToKebabCase(string optionName)
    {
        return NameFormatExtensions.ToKebabCase("--", optionName);
    }
    public ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> Configure(Action<Option<TOption>> value)
    {
        value.Invoke(option);
        return this;
    }

    public ICommandBuilderWithMapping<TCommand, TOptionHolder> AddToCommand()
    {
        command.Add(option);

        RegisterArgumentMapper();

        return commandHandlerBuilder;
    }

    private void RegisterArgumentMapper()
    {
        Action<TOptionHolder, TOption?> argumentValueMapper = propertyExpression.CreateArgumentValueMapper();

        void argumentMapper(ParseResult parsedResult, object options)
        {
            argumentValueMapper((TOptionHolder)options, parsedResult.GetValue<TOption>(option.Name));
        }

        mapperRegistration(argumentMapper);
    }
}

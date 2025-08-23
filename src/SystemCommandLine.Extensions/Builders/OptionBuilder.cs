using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

public class OptionBuilder<TCommand, TOptionHolder, TOption>(TCommand command, CommandArgumentBuilder<TCommand, TOptionHolder> commandHandlerBuilder, Expression<Func<TOptionHolder, TOption>> propertyExpression, ArgumentMapperRegistration? mapperRegistration)
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    private readonly Option<TOption> option = new(ToKebabCase(propertyExpression.GetPropertyName()));

    private static string ToKebabCase(string optionName)
    {
        return NameFormatExtensions.ToKebabCase("--", optionName);
    }
    public OptionBuilder<TCommand, TOptionHolder, TOption> Configure(Action<Option<TOption>> value)
    {
        value.Invoke(option);
        return this;
    }

    public CommandArgumentBuilder<TCommand, TOptionHolder> AddToCommand()
    {
        command.Add(option);

        if (mapperRegistration != null)
        {
            Action<TOptionHolder, TOption?> argumentMapper = propertyExpression.CreateArgumentMapper();

            mapperRegistration((parsedResult, options) =>
                argumentMapper((TOptionHolder)options, parsedResult.GetValue<TOption>(option.Name)));
        }
        return commandHandlerBuilder;
    }
}

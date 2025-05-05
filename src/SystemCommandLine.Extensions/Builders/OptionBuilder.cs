using ConsoleApp.CommandBuilders;

using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

public class OptionBuilder<TCommand, TOptionHolder, TOption>(TCommand command, CommandArgumentBuilder<TCommand, TOptionHolder> commandHandlerBuilder, Expression<Func<TOptionHolder, TOption>> propertyExpression)
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    private readonly Option<TOption> option = new(ToKebabCase(Expressions.GetPropertyName(propertyExpression)));

    private static string ToKebabCase(string optionName)
    {
        return NameExtensions.ToKebabCase("--", optionName);
    }
    public OptionBuilder<TCommand, TOptionHolder, TOption> Configure(Action<Option<TOption>> value)
    {
        value.Invoke(option);
        return this;
    }

    public CommandArgumentBuilder<TCommand, TOptionHolder> AddToCommand(ArgumentMapperRegistration? registerMapper = null)
    {
        command.Add(option);

        if (registerMapper != null)
        {
            var argumentMapper = Expressions.CreateArgumentMapper(propertyExpression);

            registerMapper((parsedResult, options) =>
                argumentMapper((TOptionHolder)options, parsedResult.GetValue<TOption>(option.Name)));
        }
        return commandHandlerBuilder;
    }
}

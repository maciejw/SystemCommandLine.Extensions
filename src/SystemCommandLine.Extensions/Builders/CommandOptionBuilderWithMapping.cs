using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

internal class CommandOptionBuilderWithMapping<TCommand, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptions, TValue>(TCommand command, ICommandBuilderWithMapping<TCommand, TOptions> commandBuilder, Expression<Func<TOptions, TValue>> propertyExpression, SymbolMapperRegistration mapperRegistration) : ICommandOptionBuilderWithMapping<TCommand, TOptions, TValue> where TCommand : Command, ICommandDefinition<TCommand>
    where TOptions : class
{
    private readonly Option<TValue> option = new(ToKebabCase(propertyExpression.GetPropertyName()));

    private static string ToKebabCase(string optionName)
    {
        return NameFormatExtensions.ToKebabCase("--", optionName);
    }
    public ICommandOptionBuilderWithMapping<TCommand, TOptions, TValue> Configure(Action<Option<TValue>> configure)
    {
        configure.Invoke(option);
        return this;
    }

    public ICommandBuilderWithMapping<TCommand, TOptions> AddToCommand()
    {
        command.Add(option);

        RegisterSymbolMapper();

        return commandBuilder;
    }

    private void RegisterSymbolMapper()
    {
        Action<TOptions, TValue?> propertyValueMapper = propertyExpression.CreatePropertyValueMapper();

        void symbolMapper(ParseResult parsedResult, object options)
        {
            propertyValueMapper((TOptions)options, parsedResult.GetValue<TValue>(option.Name));
        }

        mapperRegistration(symbolMapper);
    }
}

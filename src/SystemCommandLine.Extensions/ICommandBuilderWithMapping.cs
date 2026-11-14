using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions;

/// <summary>Defines command options and positional arguments from property expressions and registers their value mappings.</summary>
/// <typeparam name="TCommand">The command being defined.</typeparam>
/// <typeparam name="TOptions">The options type whose properties receive parsed values.</typeparam>
public interface ICommandBuilderWithMapping<TCommand, TOptions>
    where TCommand : Command, ICommandDefinition<TCommand>
    where TOptions : class
{
    /// <summary>Creates a positional argument named after and mapped to a property.</summary>
    /// <typeparam name="TValue">The property and parsed argument value type.</typeparam>
    /// <param name="propertyExpression">A direct property access. The property must have a public setter when attached.</param>
    /// <returns>An argument builder; call AddToCommand to append the argument and register its mapping.</returns>
    /// <exception cref="ArgumentException">The expression is not a property access.</exception>
    ICommandPositionalArgumentBuilderWithMapping<TCommand, TOptions, TValue> NewArgument<TValue>(Expression<Func<TOptions, TValue>> propertyExpression);

    /// <summary>Returns to the command builder for defining options and positional arguments without mapping or selecting another mapping type.</summary>
    /// <returns>The underlying builder for the same command.</returns>
    ICommandBuilder<TCommand> CommandBuilder();

    /// <summary>Creates an option builder whose name and value mapping are derived from a property.</summary>
    /// <typeparam name="TValue">The property's value type and parsed option type.</typeparam>
    /// <param name="propertyExpression">A direct property access such as <c>options =&gt; options.Name</c>. The property must have a public setter when the option is attached.</param>
    /// <returns>A mapped option builder; call its <c>AddToCommand</c> method to attach the option and register its mapper.</returns>
    /// <remarks>The property's name is converted to a long kebab-case option name, such as <c>--log-event-level</c>.</remarks>
    /// <exception cref="ArgumentException">The expression body is not a property member access.</exception>
    ICommandOptionBuilderWithMapping<TCommand, TOptions, TValue> NewOption<TValue>(Expression<Func<TOptions, TValue>> propertyExpression);
}

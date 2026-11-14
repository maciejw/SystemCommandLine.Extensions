using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions;

/// <summary>Defines command options from property expressions and registers their value mappings.</summary>
/// <typeparam name="TCommand">The command being defined.</typeparam>
/// <typeparam name="TOptionHolder">The options type whose properties receive parsed values.</typeparam>
public interface ICommandBuilderWithMapping<TCommand, TOptionHolder>
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    /// <summary>Returns to the command builder for defining options without mapping or selecting another mapping type.</summary>
    /// <returns>The underlying builder for the same command.</returns>
    ICommandBuilder<TCommand> CommandBuilder();

    /// <summary>Creates an option builder whose name and value mapping are derived from a property.</summary>
    /// <typeparam name="TOption">The property's value type and parsed option type.</typeparam>
    /// <param name="propertyExpression">A direct property access such as <c>options =&gt; options.Name</c>. The property must have a public setter when the option is attached.</param>
    /// <returns>A mapped option builder; call its <c>AddToCommand</c> method to attach the option and register its mapper.</returns>
    /// <remarks>The property's name is converted to a long kebab-case option name, such as <c>--log-event-level</c>.</remarks>
    /// <exception cref="ArgumentException">The expression body is not a property member access.</exception>
    ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> NewOption<TOption>(Expression<Func<TOptionHolder, TOption>> propertyExpression);
}

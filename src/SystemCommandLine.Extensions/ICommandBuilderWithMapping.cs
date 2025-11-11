using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions;

/// <summary>
/// Defines a contract for building command objects with mapped options, enabling fluent configuration of commands and
/// their associated option holders.
/// </summary>
/// <remarks>This interface is typically used in scenarios where commands require strongly-typed option mapping
/// for configuration. It provides methods to access the underlying command builder and to create new option mappings
/// based on properties of the option holder.</remarks>
/// <typeparam name="TCommand">The type of command being constructed. Must implement both Command and <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
/// <typeparam name="TOptionHolder">The type that holds the options to be mapped to the command. Must be a reference type.</typeparam>
public interface ICommandBuilderWithMapping<TCommand, TOptionHolder>
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    /// <summary>
    /// Creates and returns a builder for constructing command instances of type <typeparamref name="TCommand"/>.
    /// </summary>
    /// <returns>An <see cref="ICommandBuilder{TCommand}"/> that can be used to configure and build command objects.</returns>
    ICommandBuilder<TCommand> CommandBuilder();
    /// <summary>
    /// Creates a new option mapping for the specified property of the option holder type.
    /// </summary>
    /// <remarks>Use this method to define a command-line option that is bound to a property on the option
    /// holder. The returned builder can be used to configure option details such as name, description, and mapping
    /// behavior.</remarks>
    /// <typeparam name="TOption">The type of the option to be mapped.</typeparam>
    /// <param name="propertyExpression">An expression that specifies the property of the option holder to associate with the new option. Must refer to a
    /// readable property.</param>
    /// <returns>An argument builder that allows further configuration and mapping of the newly created option.</returns>
    ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> NewOption<TOption>(Expression<Func<TOptionHolder, TOption>> propertyExpression);
}
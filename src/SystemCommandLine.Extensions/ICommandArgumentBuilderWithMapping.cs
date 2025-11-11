using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>
/// Provides a builder interface for configuring and mapping a command-line option to a command argument, supporting
/// fluent configuration and integration with command builders.
/// </summary>
/// <remarks>Use this interface to fluently configure command-line options and associate them with command
/// arguments. It enables chaining of configuration methods and integration with command builders for advanced
/// command-line parsing scenarios.</remarks>
/// <typeparam name="TCommand">The type of the command to which the option will be mapped. Must implement Command and <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
/// <typeparam name="TOptionHolder">The type that holds the options for the command. Must be a reference type.</typeparam>
/// <typeparam name="TOption">The type of the value represented by the option.</typeparam>
public interface ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption>
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    /// <summary>
    /// Gets the option value associated with the current instance.
    /// </summary>
    Option<TOption> Option { get; }
    /// <summary>
    /// Adds the current option holder to the command builder and returns an interface for further mapping or
    /// configuration.
    /// </summary>
    /// <returns>An interface that allows additional mapping or configuration of the command and option holder.</returns>
    ICommandBuilderWithMapping<TCommand, TOptionHolder> AddToCommand();
    /// <summary>
    /// Configures the option by applying the specified action to its definition.
    /// </summary>
    /// <remarks>Use this method to modify the option's settings, such as its description, default value, or
    /// validation rules, prior to mapping it to the command. The configuration is applied before the option is bound to
    /// the command's logic.</remarks>
    /// <param name="value">An action to perform on the option definition. This action can be used to customize properties or behaviors of
    /// the option before it is mapped.</param>
    /// <returns>An interface that allows further configuration and mapping of the command argument option.</returns>
    ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> Configure(Action<Option<TOption>> value);
}
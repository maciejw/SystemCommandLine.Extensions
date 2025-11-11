using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>
/// Defines a builder for configuring and adding a typed option argument to a command within a command-line application.
/// </summary>
/// <remarks>Use this interface to fluently configure option arguments and attach them to commands.
/// Implementations typically allow chaining of configuration methods for building complex command-line
/// interfaces.</remarks>
/// <typeparam name="TCommand">The type of the command to which the option argument will be added. Must implement both Command and
/// <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
/// <typeparam name="TOption">The type of the option value associated with the argument.</typeparam>
public interface ICommandArgumentBuilder<TCommand, TOption> where TCommand : Command, IUseCommandBuilder<TCommand>
{
    /// <summary>
    /// Gets the option value associated with the current instance.
    /// </summary>
    /// <remarks>The returned <see cref="Option{TOption}"/> may represent either a present or absent value,
    /// depending on the state of the instance. Use the properties and methods of <see cref="Option{TOption}"/> to
    /// safely access the underlying value.</remarks>
    Option<TOption> Option { get; }
    /// <summary>
    /// Adds the current configuration to the command builder and returns the updated builder instance.
    /// </summary>
    /// <returns>An <see cref="ICommandBuilder{TCommand}"/> representing the command builder with the current configuration
    /// applied.</returns>
    ICommandBuilder<TCommand> AddToCommand();
    /// <summary>
    /// Configures the option by applying the specified action to its definition.
    /// </summary>
    /// <remarks>Use this method to customize the option's behavior, such as setting default values,
    /// descriptions, or validation rules. The configuration is applied before the command is executed.</remarks>
    /// <param name="value">An action to perform on the option's definition. Cannot be null.</param>
    /// <returns>An <see cref="ICommandArgumentBuilder{TCommand, TOption}"/> instance that can be used for further configuration.</returns>
    ICommandArgumentBuilder<TCommand, TOption> Configure(Action<Option<TOption>> value);
}
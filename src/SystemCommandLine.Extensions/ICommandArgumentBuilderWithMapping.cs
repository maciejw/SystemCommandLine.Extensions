using System.ComponentModel;
using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>Configures an option whose parsed value is mapped to a property on an options object.</summary>
/// <typeparam name="TCommand">The command that will own the option.</typeparam>
/// <typeparam name="TOptions">The options type containing the target property.</typeparam>
/// <typeparam name="TValue">The parsed value and target property type.</typeparam>
[Obsolete("Renamed to ICommandOptionBuilderWithMapping. Update your references to use ICommandOptionBuilderWithMapping.", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ICommandArgumentBuilderWithMapping<TCommand, TOptions, TValue>
    where TCommand : Command, ICommandDefinition<TCommand>
    where TOptions : class
{
    /// <summary>Attaches the option to the command and registers its property-value mapper.</summary>
    /// <returns>The mapped command builder for defining further mapped options.</returns>
    /// <exception cref="ArgumentException">The selected property's setter is not public or cannot be used to assign the mapped value.</exception>
    ICommandBuilderWithMapping<TCommand, TOptions> AddToCommand();

    /// <summary>Applies configuration to the underlying option definition.</summary>
    /// <param name="value">The callback that configures the option's description, default, validation, or other settings.</param>
    /// <returns>This mapped option builder for further configuration or attachment.</returns>
    ICommandArgumentBuilderWithMapping<TCommand, TOptions, TValue> Configure(Action<Option<TValue>> value);
}

using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>Configures an option whose parsed value is mapped to a property on an options object.</summary>
/// <typeparam name="TCommand">The command that will own the option.</typeparam>
/// <typeparam name="TOptionHolder">The options type containing the target property.</typeparam>
/// <typeparam name="TOption">The parsed value and target property type.</typeparam>
public interface ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption>
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    /// <summary>Attaches the option to the command and registers its property-value mapper.</summary>
    /// <returns>The mapped command builder for defining further mapped options.</returns>
    /// <exception cref="ArgumentException">The selected property's setter is not public or cannot be used to assign the mapped value.</exception>
    ICommandBuilderWithMapping<TCommand, TOptionHolder> AddToCommand();

    /// <summary>Applies configuration to the underlying option definition.</summary>
    /// <param name="value">The callback that configures the option's description, default, validation, or other settings.</param>
    /// <returns>This mapped option builder for further configuration or attachment.</returns>
    ICommandArgumentBuilderWithMapping<TCommand, TOptionHolder, TOption> Configure(Action<Option<TOption>> value);
}

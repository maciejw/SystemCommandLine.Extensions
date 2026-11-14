using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>Configures an option before attaching it to a command, without binding it to an options object.</summary>
/// <typeparam name="TCommand">The command that will own the option.</typeparam>
/// <typeparam name="TValue">The parsed option value type.</typeparam>
public interface ICommandOptionBuilder<TCommand, TValue> where TCommand : Command, ICommandDefinition<TCommand>
{
    /// <summary>Attaches the configured option to its command.</summary>
    /// <returns>The command builder for defining further options.</returns>
    ICommandBuilder<TCommand> AddToCommand();

    /// <summary>Applies configuration to the underlying option definition.</summary>
    /// <param name="configure">The callback that configures the option's description, default, validation, or other settings.</param>
    /// <returns>This option builder for further configuration or attachment.</returns>
    ICommandOptionBuilder<TCommand, TValue> Configure(Action<Option<TValue>> configure);
}

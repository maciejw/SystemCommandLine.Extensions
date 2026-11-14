#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.CommandLine;

using System.ComponentModel;
using SystemCommandLine.Extensions.Builders;

namespace SystemCommandLine.Extensions;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>Use CommandBuilderExtensions to start fluent command definition.</summary>
[Obsolete("Renamed to CommandBuilderExtensions. Extension calls to command.UseCommandBuilder() are unchanged; update explicit static calls to CommandBuilderExtensions.UseCommandBuilder(command).", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IUseCommandBuilderExtensions
{
    /// <summary>Creates an option-definition builder for an existing command.</summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command to which options will be attached.</param>
    /// <returns>A builder that defines options on the supplied command instance.</returns>
    /// <remarks>This configures a command definition; it does not register the command with dependency injection.</remarks>
    [Obsolete("Use CommandBuilderExtensions.UseCommandBuilder(command), or command.UseCommandBuilder().", error: true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ICommandBuilder<TCommand> UseCommandBuilder<TCommand>(TCommand command)
       where TCommand : Command, ICommandDefinition<TCommand>
    {
        return new CommandBuilder<TCommand>(command);
    }
}

#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.CommandLine;

using SystemCommandLine.Extensions.Builders;

namespace SystemCommandLine.Extensions;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>Starts fluent option definition on commands that provide a command factory.</summary>
public static class IUseCommandBuilderExtensions
{
    /// <summary>Creates an option-definition builder for an existing command.</summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command to which options will be attached.</param>
    /// <returns>A builder that defines options on the supplied command instance.</returns>
    /// <remarks>This configures a command definition; it does not register the command with dependency injection.</remarks>
    public static ICommandBuilder<TCommand> UseCommandBuilder<TCommand>(this TCommand command)
       where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        return new CommandBuilder<TCommand>(command);
    }
}

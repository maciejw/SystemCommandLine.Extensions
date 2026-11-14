#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.CommandLine;

using SystemCommandLine.Extensions.Builders;

namespace SystemCommandLine.Extensions;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>Starts fluent option and positional argument definition on commands that provide a command factory.</summary>
public static class CommandBuilderExtensions
{
    /// <summary>Creates an option and positional argument builder for an existing command.</summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command to which options and positional arguments will be attached.</param>
    /// <returns>A builder that defines options and positional arguments on the supplied command instance.</returns>
    /// <remarks>This configures a command definition; it does not register the command with dependency injection.</remarks>
    public static ICommandBuilder<TCommand> UseCommandBuilder<TCommand>(this TCommand command)
       where TCommand : Command, ICommandDefinition<TCommand>
    {
        return new CommandBuilder<TCommand>(command);
    }
}

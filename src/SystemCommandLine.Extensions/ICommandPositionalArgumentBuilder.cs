using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>Configures a positional argument before attaching it to a command.</summary>
/// <typeparam name="TCommand">The command that owns the argument.</typeparam>
/// <typeparam name="TValue">The parsed argument value type.</typeparam>
public interface ICommandPositionalArgumentBuilder<TCommand, TValue>
    where TCommand : Command, IUseCommandBuilder<TCommand>
{
    /// <summary>Configures the argument's name, description, arity, defaults, or validation.</summary>
    /// <param name="configure">The callback applied to the argument definition.</param>
    /// <returns>This argument builder.</returns>
    ICommandPositionalArgumentBuilder<TCommand, TValue> Configure(Action<Argument<TValue>> configure);

    /// <summary>Appends the argument to the command. Arguments are parsed in attachment order.</summary>
    /// <returns>The originating command builder.</returns>
    ICommandBuilder<TCommand> AddToCommand();
}


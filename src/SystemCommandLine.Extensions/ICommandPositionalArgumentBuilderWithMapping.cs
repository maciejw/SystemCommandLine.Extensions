using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>Configures a positional argument before attaching it to a command and registering its property mapping.</summary>
/// <typeparam name="TCommand">The command that owns the argument.</typeparam>
/// <typeparam name="TOptions">The type containing the mapped property.</typeparam>
/// <typeparam name="TValue">The parsed argument value type.</typeparam>
public interface ICommandPositionalArgumentBuilderWithMapping<TCommand, TOptions, TValue>
    where TCommand : Command, ICommandDefinition<TCommand>
    where TOptions : class
{
    /// <summary>Configures the argument's name, description, arity, defaults, or validation.</summary>
    /// <param name="configure">The callback applied to the argument definition.</param>
    /// <returns>This argument builder.</returns>
    ICommandPositionalArgumentBuilderWithMapping<TCommand, TOptions, TValue> Configure(Action<Argument<TValue>> configure);

    /// <summary>Appends the argument to the command and registers its property mapping. Arguments are parsed in attachment order.</summary>
    /// <returns>The originating command builder.</returns>
    ICommandBuilderWithMapping<TCommand, TOptions> AddToCommand();
}


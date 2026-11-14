using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace SystemCommandLine.Extensions;

/// <summary>Registers a command's handler, bound options, and child relationships.</summary>
/// <typeparam name="TCommand">The command type whose registration is being configured.</typeparam>
/// <remarks>
/// Obtain this builder from AddCommand or AddRootCommand and complete registration before
/// building the service provider. Command definitions remain owned by their factories.
/// </remarks>
public sealed class CommandRegistrationBuilder<TCommand>
    where TCommand : Command, IUseCommandBuilder<TCommand>
{
    private readonly List<Type> subcommands = [];
    private readonly HashSet<Type> boundOptions = [];
    private Action<TCommand, IServiceProvider>? setHandler;

    internal CommandRegistrationBuilder(IServiceCollection services) => Services = services;

    /// <summary>Gets the service collection for further application registration.</summary>
    /// <value>The original service collection passed to the command registration method.</value>
    public IServiceCollection Services { get; }

    /// <summary>Registers and selects a synchronous handler for this command.</summary>
    /// <typeparam name="THandler">The synchronous handler type to resolve when the command is invoked.</typeparam>
    /// <returns>This command's registration builder.</returns>
    /// <remarks>
    /// Registers the handler as a singleton if the service type is not already registered.
    /// The last WithHandler or WithAsyncHandler call selects the handler, allowing application registration
    /// to override the command's defaults. Handler selection replaces any factory-defined action when the command is created.
    /// </remarks>
    public CommandRegistrationBuilder<TCommand> WithHandler<THandler>()
        where THandler : SynchronousCommandLineAction
    {
        Services.TryAddSingleton<THandler>();
        setHandler = (command, sp) => command.SetHandler<TCommand, THandler>(sp);
        return this;
    }

    /// <summary>Registers and selects an asynchronous handler for this command.</summary>
    /// <typeparam name="THandler">The asynchronous handler type to resolve when the command is invoked.</typeparam>
    /// <returns>This command's registration builder.</returns>
    /// <remarks>
    /// Registers the handler as a singleton if the service type is not already registered.
    /// The last WithHandler or WithAsyncHandler call selects the handler, allowing application registration
    /// to override the command's defaults. Handler selection replaces any factory-defined action when the command is created.
    /// </remarks>
    public CommandRegistrationBuilder<TCommand> WithAsyncHandler<THandler>()
        where THandler : AsynchronousCommandLineAction
    {
        Services.TryAddSingleton<THandler>();
        setHandler = (command, sp) => command.SetAsyncHandler<TCommand, THandler>(sp);
        return this;
    }

    /// <summary>Binds parsed values to the options type mapped by this command's factory.</summary>
    /// <typeparam name="TOptions">The options type used by the command's property mappings.</typeparam>
    /// <returns>This command's registration builder.</returns>
    /// <remarks>
    /// Registers the options type with the options system and applies the command's mappers when an options instance
    /// is configured. This requires a registered <see cref="ParseResult"/>, normally supplied by AddRootCommand.
    /// Repeated binding of the same options type on this builder is ignored. This method does not define command-line options.
    /// </remarks>
    public CommandRegistrationBuilder<TCommand> BindOptions<TOptions>() where TOptions : class
    {
        if (boundOptions.Add(typeof(TOptions)))
        {
            Services.AddOptions<TOptions>()
                .Configure<TCommand, IOptions<CommandArgumentMapper<TCommand>>, ParseResult>((options, command, mappers, parseResult) =>
                {
                    foreach (var mapper in mappers.Value)
                    {
                        mapper(parseResult, options);
                    }
                });
        }

        return this;
    }

    /// <summary>Attaches a separately registered child command and returns this parent's builder.</summary>
    /// <typeparam name="TChild">The child command type to resolve from the service provider.</typeparam>
    /// <returns>This parent's registration builder; subsequent calls still configure the parent.</returns>
    /// <remarks>
    /// Children are resolved from DI after the parent's factory runs and before parsing.
    /// This does not register the child. Repeated declarations of the same child type on this builder are ignored,
    /// as are child instances already attached by the factory. Missing child registrations fail when the parent is
    /// first resolved. Relationships must be acyclic; only direct self-parenting is rejected during registration.
    /// </remarks>
    /// <exception cref="InvalidOperationException"><typeparamref name="TChild"/> is the same type as <typeparamref name="TCommand"/>.</exception>
    public CommandRegistrationBuilder<TCommand> AddSubCommand<TChild>()
        where TChild : Command, IUseCommandBuilder<TChild>
    {
        if (typeof(TChild) == typeof(TCommand))
        {
            throw new InvalidOperationException("A command cannot be its own subcommand.");
        }

        if (!subcommands.Contains(typeof(TChild)))
        {
            subcommands.Add(typeof(TChild));
        }

        return this;
    }

    internal TCommand CreateCommand(IServiceProvider serviceProvider)
    {
        var mappers = serviceProvider.GetRequiredService<IOptions<CommandArgumentMapper<TCommand>>>().Value;
        var command = TCommand.CommandFactory(serviceProvider, mappers.Add);
        setHandler?.Invoke(command, serviceProvider);

        foreach (var childType in subcommands)
        {
            var child = (Command)serviceProvider.GetRequiredService(childType);
            if (!command.Subcommands.Any(existing => ReferenceEquals(existing, child)))
            {
                command.Add(child);
            }
        }

        return command;
    }
}

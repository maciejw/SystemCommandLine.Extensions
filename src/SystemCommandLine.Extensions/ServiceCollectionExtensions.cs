using System.CommandLine;
using System.CommandLine.Invocation;

using SystemCommandLine.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>Registers command factories and root parsing, and connects DI-resolved command handlers.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers a singleton root command and its parse result, and returns its registration builder.</summary>
    /// <typeparam name="TRootCommand">The root command type providing a factory and optional registration defaults.</typeparam>
    /// <param name="services">The service collection to register the command and parse result with.</param>
    /// <param name="args">The command-line arguments to parse when the parse result is first resolved.</param>
    /// <returns>The root's registration builder for declaring children, selecting a handler, or binding options.</returns>
    /// <remarks>
    /// Invokes the command's registration hook immediately. The command and <see cref="ParseResult"/> are created
    /// lazily as singletons; external children are attached before parsing. This method does not invoke the command.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="args"/> is null.</exception>
    public static CommandRegistrationBuilder<TRootCommand> AddRootCommand<TRootCommand>(this IServiceCollection services, string[] args)
    where TRootCommand : RootCommand, IUseCommandBuilder<TRootCommand>
    {
        ArgumentNullException.ThrowIfNull(args);
        var builder = services.AddCommand<TRootCommand>();
        services.AddSingleton(sp =>
        {
            return sp.GetRequiredService<TRootCommand>().Parse(args);
        });
        return builder;
    }

    /// <summary>Registers a singleton command using its factory and returns its registration builder.</summary>
    /// <typeparam name="TCommand">The command type providing a factory and optional registration defaults.</typeparam>
    /// <param name="services">The service collection to register the command with.</param>
    /// <returns>The same registration builder passed to the command's registration hook, ready for application-level choices.</returns>
    /// <remarks>
    /// Calls the command's registration hook once per registration call without constructing the command or handler.
    /// Finish configuring the builder before building the service provider. Register each command once; a child
    /// relationship alone does not register that child or create a parse result.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is null.</exception>
    public static CommandRegistrationBuilder<TCommand> AddCommand<TCommand>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<CommandArgumentMapper<TCommand>>();
        var builder = new CommandRegistrationBuilder<TCommand>(services);
        services.AddSingleton(builder.CreateCommand);
        TCommand.ConfigureRegistration(builder);
        return builder;
    }

    /// <summary>Sets the command's action to invoke an asynchronous handler resolved from DI.</summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="THandler">The asynchronous handler type.</typeparam>
    /// <param name="command">The command whose action is replaced.</param>
    /// <param name="sp">The service provider from which to resolve the handler at invocation time.</param>
    /// <returns>The supplied command instance.</returns>
    /// <remarks>The handler must already be registered with the provider. The parse result and invocation cancellation token are forwarded to it.</remarks>
    public static TCommand SetAsyncHandler<TCommand, THandler>(this TCommand command, IServiceProvider sp) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : AsynchronousCommandLineAction
    {
        command.SetAction((pr, ct) => sp.GetRequiredService<THandler>().InvokeAsync(pr, ct));
        return command;
    }
    /// <summary>Sets the command's action to invoke a synchronous handler resolved from DI.</summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="THandler">The synchronous handler type.</typeparam>
    /// <param name="command">The command whose action is replaced.</param>
    /// <param name="sp">The service provider from which to resolve the handler at invocation time.</param>
    /// <returns>The supplied command instance.</returns>
    /// <remarks>The handler must already be registered with the provider. The command's parse result is forwarded to it.</remarks>
    public static TCommand SetHandler<TCommand, THandler>(this TCommand command, IServiceProvider sp) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : SynchronousCommandLineAction
    {
        command.SetAction(pr => sp.GetRequiredService<THandler>().Invoke(pr));
        return command;
    }

}

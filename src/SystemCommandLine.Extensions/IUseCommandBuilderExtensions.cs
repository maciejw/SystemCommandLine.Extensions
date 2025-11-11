using Microsoft.Extensions.DependencyInjection;

using System.CommandLine;
using System.CommandLine.Invocation;

using SystemCommandLine.Extensions.Builders;

namespace SystemCommandLine.Extensions;

/// <summary>
/// Provides extension methods for configuring and composing command instances using fluent builders and dependency
/// injection.
/// </summary>
/// <remarks>These extensions enable dynamic registration of subcommands and handlers for command types,
/// supporting both synchronous and asynchronous execution patterns. Methods in this class facilitate integration with
/// dependency injection frameworks, allowing commands to be composed and configured at runtime based on registered
/// services.</remarks>
public static class IUseCommandBuilderExtensions
{
    /// <summary>
    /// Creates a command builder for the specified command instance, enabling fluent configuration and execution of the
    /// command.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be wrapped by the command builder. Must inherit from Command and implement
    /// <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
    /// <param name="command">The command instance to be wrapped by the command builder. Cannot be null.</param>
    /// <returns>An <see cref="ICommandBuilder{TCommand}"/> that provides a fluent interface for configuring and executing the specified
    /// command.</returns>
    public static ICommandBuilder<TCommand> UseCommandBuilder<TCommand>(this TCommand command)
       where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        return new CommandBuilder<TCommand>(command);
    }
    /// <summary>
    /// Adds all subcommands registered for the specified command type to the given command instance using the provided
    /// service provider.
    /// </summary>
    /// <remarks>This method retrieves all services of type <see cref="ISubCommandOf{TCommand}"/> from the service provider
    /// and adds them as subcommands to the specified command. This is useful for dynamically composing command
    /// hierarchies based on dependency injection configuration.</remarks>
    /// <typeparam name="TCommand">The type of the command to which subcommands will be added. Must implement both Command and
    /// <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
    /// <param name="command">The command instance to which subcommands will be added.</param>
    /// <param name="serviceProvider">The service provider used to resolve subcommands implementing <see cref="ISubCommandOf{TCommand}"/>.</param>
    /// <returns>The original command instance with all discovered subcommands added.</returns>
    public static TCommand AddRegisteredSubCommands<TCommand>(this TCommand command, IServiceProvider serviceProvider)
       where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        foreach (var item in serviceProvider.GetServices<ISubCommandOf<TCommand>>())
        {
            if (item is Command subCommand)
            {
                command.Add(subCommand);
            }
        }
        return command;
    }
    /// <summary>
    /// Associates an asynchronous command-line handler of type <typeparamref name="THandler"/> with the specified
    /// command using dependency injection.
    /// </summary>
    /// <remarks>This method sets the command's action to invoke the specified asynchronous handler, which is
    /// resolved from the provided service provider. The handler will be executed when the command is invoked. Ensure
    /// that <typeparamref name="THandler"/> is registered with the service provider before calling this
    /// method.</remarks>
    /// <typeparam name="TCommand">The type of the command to configure. Must implement <see cref="Command"/> and <see
    /// cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
    /// <typeparam name="THandler">The type of the asynchronous handler to invoke for the command. Must inherit from <see
    /// cref="AsynchronousCommandLineAction"/>.</typeparam>
    /// <param name="command">The command instance to configure with the asynchronous handler.</param>
    /// <param name="sp">The <see cref="IServiceProvider"/> used to resolve the handler instance.</param>
    /// <returns>The command instance with the asynchronous handler configured.</returns>
    public static TCommand SetAsyncHandler<TCommand, THandler>(this TCommand command, IServiceProvider sp) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : AsynchronousCommandLineAction
    {
        command.SetAction((pr, ct) => sp.GetRequiredService<THandler>().InvokeAsync(pr, ct));
        return command;
    }
    /// <summary>
    /// Configures the specified command to use a synchronous handler resolved from the provided service provider.
    /// </summary>
    /// <remarks>This method sets the command's action to invoke the resolved handler when the command is
    /// executed. The handler is obtained from the service provider at runtime, enabling dependency injection for
    /// command actions.</remarks>
    /// <typeparam name="TCommand">The type of the command to configure. Must implement <see cref="Command"/> and <see
    /// cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
    /// <typeparam name="THandler">The type of the synchronous command-line action handler to associate with the command. Must inherit from <see
    /// cref="SynchronousCommandLineAction"/>.</typeparam>
    /// <param name="command">The command instance to configure with the specified handler.</param>
    /// <param name="sp">The service provider used to resolve the handler instance of type <typeparamref name="THandler"/>.</param>
    /// <returns>The same command instance, configured to use the specified handler.</returns>
    public static TCommand SetHandler<TCommand, THandler>(this TCommand command, IServiceProvider sp) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : SynchronousCommandLineAction
    {
        command.SetAction(pr => sp.GetRequiredService<THandler>().Invoke(pr));
        return command;
    }
}

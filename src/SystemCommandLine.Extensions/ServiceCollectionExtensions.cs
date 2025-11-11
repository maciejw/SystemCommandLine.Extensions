using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

using SystemCommandLine.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering commands, options, handlers, and subcommands in an <see cref="IServiceCollection"/> for
/// command-line applications using dependency injection.
/// </summary>
/// <remarks>These extension methods facilitate the configuration and registration of command types, their
/// options, argument mapping, and handlers within the dependency injection container. Use these methods to enable
/// automatic binding and resolution of command-line components, supporting both synchronous and asynchronous command
/// execution scenarios.</remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers command options and argument mapping for the specified command type in the dependency injection
    /// container.
    /// </summary>
    /// <remarks>This method enables automatic binding of options and argument mapping for the specified
    /// command type, facilitating configuration and dependency injection for command-line applications.</remarks>
    /// <typeparam name="TCommand">The type of command to bind options to. Must inherit from Command and implement <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
    /// <typeparam name="TOptions">The type of options to bind to the command. Must be a reference type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the command options and argument mapper will be added.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance with the command options and argument mapper registrations added.</returns>
    public static IServiceCollection AddBoundToCommandOptions<TCommand, TOptions>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
        where TOptions : class
    {
        return services
            .AddOptions<TOptions>().BindCommand<TCommand, TOptions>().Services
            .AddOptions<CommandArgumentMapper<TCommand>>().Services;
    }

    /// <summary>
    /// Adds the specified root command to the service collection and configures it to parse the provided command-line
    /// arguments at startup.
    /// </summary>
    /// <remarks>This method registers <see cref="ParseResult"/> returned by <see cref="Command.Parse(string, ParserConfiguration?)"/> method is invoked with the
    /// specified arguments when resolving dependencies. Use this to enable command-line parsing and handling in
    /// applications that utilize dependency injection.</remarks>
    /// <typeparam name="TRootCommand">The type of the root command to register. Must inherit from <see cref="RootCommand"/> and implement
    /// <see cref="IUseCommandBuilder{TRootCommand}"/>.</typeparam>
    /// <param name="services">The service collection to which the root command and its dependencies will be added.</param>
    /// <param name="args">The array of command-line arguments to be parsed by the root command during application startup.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance with the root command and its parsed result registered for dependency
    /// injection.</returns>
    public static IServiceCollection AddRootCommand<TRootCommand>(this IServiceCollection services, string[] args)
    where TRootCommand : RootCommand, IUseCommandBuilder<TRootCommand>
    {
        return services.AddCommand<TRootCommand>().AddSingleton(sp =>
        {
            return sp.GetRequiredService<TRootCommand>().Parse(args);
        });
    }
    /// <summary>
    /// Registers a command of type <typeparamref name="TCommand"/> as a singleton in the service collection.
    /// </summary>
    /// <remarks>Use this method to make a command available for dependency injection as a singleton. The
    /// command is constructed using its associated command builder and argument mapper registration.</remarks>
    /// <typeparam name="TCommand">The type of command to register. Must inherit from <see cref="Command"/> and implement <see
    /// cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the command will be added.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance with the command registration added.</returns>
    public static IServiceCollection AddCommand<TCommand>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        RegisterImplementedSubCommandInterfaces<TCommand>(services);

        return services.AddSingleton(sp =>
        {
            return TCommand.CommandFactory(sp, GetArgumentMapperRegistration<TCommand>(sp));
        });
    }

    /// <summary>
    /// Registers a command and its asynchronous handler in the dependency injection container.
    /// </summary>
    /// <remarks>This method configures the command and its handler for use with dependency injection,
    /// enabling asynchronous command execution. The command is created using its CommandFactory and is associated with
    /// the specified handler via SetAsyncHandler. This is typically used in command-line applications to support
    /// extensible, DI-based command registration.</remarks>
    /// <typeparam name="TCommand">The type of the command to register. Must implement both Command and <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
    /// <typeparam name="THandler">The type of the asynchronous handler for the command. Must inherit from <see cref="AsynchronousCommandLineAction"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the command and handler will be added.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance with the command and its asynchronous handler registered as singletons.</returns>
    public static IServiceCollection AddCommandWithAsyncHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
        where THandler : AsynchronousCommandLineAction
    {
        RegisterImplementedSubCommandInterfaces<TCommand>(services);

        return services.AddSingleton<THandler>().AddSingleton(sp =>
        {
            return TCommand.CommandFactory(sp, GetArgumentMapperRegistration<TCommand>(sp)).SetAsyncHandler<TCommand, THandler>(sp);
        });
    }
    /// <summary>
    /// Registers a command and its associated handler with the dependency injection container for use in command-line
    /// applications.
    /// </summary>
    /// <remarks>This method adds both the command and its handler as singletons to the service collection.
    /// The command is configured to use the specified handler for execution. Use this method to enable dependency
    /// injection of command and handler types in command-line applications.</remarks>
    /// <typeparam name="TCommand">The type of the command to register. Must inherit from Command and implement <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
    /// <typeparam name="THandler">The type of the handler that executes the command. Must inherit from <see cref="SynchronousCommandLineAction"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the command and handler will be added.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance with the command and handler registrations added.</returns>
    public static IServiceCollection AddCommandWithHandler<TCommand, THandler>(this IServiceCollection services) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : SynchronousCommandLineAction
    {
        RegisterImplementedSubCommandInterfaces<TCommand>(services);

        return services.AddSingleton<THandler>().AddSingleton(sp =>
        {
            return TCommand.CommandFactory(sp, GetArgumentMapperRegistration<TCommand>(sp)).SetHandler<TCommand, THandler>(sp);
        });
    }

    private static void RegisterImplementedSubCommandInterfaces<TCommand>(IServiceCollection services) where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        var implType = typeof(TCommand);
        var subCommandInterfaces = implType
           .GetInterfaces()
           .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISubCommandOf<>))
           .ToArray();

        if (subCommandInterfaces.Length > 0)
        {
            foreach (var subCommandInterface in subCommandInterfaces)
            {
                services.TryAddSingleton(subCommandInterface, sp => sp.GetRequiredService<TCommand>());
            }
        }
    }

    private static OptionsBuilder<TOptions> BindCommand<TCommand, TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
        where TCommand : Command, IUseCommandBuilder<TCommand>
        where TOptions : class
    {
        return optionsBuilder.Configure<TCommand, IOptions<CommandArgumentMapper<TCommand>>, ParseResult>((options, command, commandArgumentMappers, parseResult) =>
        {
            foreach (ArgumentMapper argumentMapper in commandArgumentMappers.Value)
            {
                argumentMapper(parseResult, options);
            }
        });
    }
    private static ArgumentMapperRegistration GetArgumentMapperRegistration<TCommand>(IServiceProvider sp) where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        var options = sp.GetService<IOptions<CommandArgumentMapper<TCommand>>>();
        if (options != null)
        {
            return options.Value.Add;
        }
        else
        {
            return _ => { throw new InvalidOperationException($"No argument mapper registration found for {typeof(TCommand)}. Use services.AddBoundToCommandOptions<...>(...) during registration."); };
        }
    }

}

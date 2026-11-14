using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace SystemCommandLine.Extensions;

public sealed class CommandRegistrationBuilder<TCommand>
    where TCommand : Command, IUseCommandBuilder<TCommand>
{
    private readonly List<Type> subcommands = [];
    private readonly HashSet<Type> boundOptions = [];
    private Action<TCommand, IServiceProvider>? setHandler;

    internal CommandRegistrationBuilder(IServiceCollection services) => Services = services;

    public IServiceCollection Services { get; }

    public CommandRegistrationBuilder<TCommand> WithHandler<THandler>()
        where THandler : SynchronousCommandLineAction
    {
        Services.TryAddSingleton<THandler>();
        setHandler = (command, sp) => command.SetHandler<TCommand, THandler>(sp);
        return this;
    }

    public CommandRegistrationBuilder<TCommand> WithAsyncHandler<THandler>()
        where THandler : AsynchronousCommandLineAction
    {
        Services.TryAddSingleton<THandler>();
        setHandler = (command, sp) => command.SetAsyncHandler<TCommand, THandler>(sp);
        return this;
    }

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

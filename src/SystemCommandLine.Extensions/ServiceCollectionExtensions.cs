using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

using SystemCommandLine.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ServiceCollectionExtensions
{
    private static OptionsBuilder<TOptions> BindCommand<TCommand, TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
        where TCommand : Command, IUseCommandBuilder<TCommand>
        where TOptions : class, new()
    {
        optionsBuilder.Configure<TCommand, IOptions<CommandArgumentMapper<TCommand>>, ParseResult>((options, command, commandArgumentMappers, parseResult) =>
        {
            foreach (OptionMapper optionMapping in commandArgumentMappers.Value)
            {
                optionMapping(parseResult, options);
            }
        });
        return optionsBuilder;
    }
    public static IServiceCollection AddBoundToCommandOptions<TCommand, TOptions>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
        where TOptions : class, new()
    {

        services
            .AddOptions<TOptions>().BindCommand<TCommand, TOptions>().Services
            .AddOptions<CommandArgumentMapper<TCommand>>();
        return services;
    }

    public static IServiceCollection AddCommand<TCommand>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        services.AddSingleton(sp =>
        {
            ArgumentMapperRegistration register = GetArgumentMapperRegistration<TCommand>(sp);
            return TCommand.CommandFactory(sp, register);
        });

        return services;
    }

    public static IServiceCollection AddCommandWithAsyncHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
        where THandler : AsynchronousCommandLineAction
    {
        services.AddSingleton<THandler>();

        services.AddSingleton(sp =>
        {
            ArgumentMapperRegistration register = GetArgumentMapperRegistration<TCommand>(sp);
            return TCommand.CommandFactory(sp, register).SetAsyncHandler<TCommand, THandler>(sp);
        });

        return services;
    }

    public static IServiceCollection AddCommandWithHandler<TCommand, THandler>(this IServiceCollection services) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : SynchronousCommandLineAction
    {
        services.AddSingleton(sp =>
        {
            ArgumentMapperRegistration register = GetArgumentMapperRegistration<TCommand>(sp);
            return TCommand.CommandFactory(sp, register).SetHandler<TCommand, THandler>(sp);
        });

        return services;
    }

    public static TCommand SetAsyncHandler<TCommand, THandler>(this TCommand command, IServiceProvider sp) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : AsynchronousCommandLineAction
    {
        command.SetAction((pr, t) => sp.GetRequiredService<THandler>().InvokeAsync(pr, t));

        return command;
    }
    public static TCommand SetHandler<TCommand, THandler>(this TCommand command, IServiceProvider sp) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : SynchronousCommandLineAction
    {
        command.SetAction(pr => sp.GetRequiredService<THandler>().Invoke(pr));

        return command;
    }

    private static ArgumentMapperRegistration GetArgumentMapperRegistration<TCommand>(IServiceProvider sp) where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        return sp.GetRequiredService<IOptions<CommandArgumentMapper<TCommand>>>().Value.Add;
    }
}

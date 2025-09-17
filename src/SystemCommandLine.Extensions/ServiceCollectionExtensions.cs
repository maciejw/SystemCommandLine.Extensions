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
    public static IServiceCollection AddBoundToCommandOptions<TCommand, TOptions>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
        where TOptions : class
    {
        return services
            .AddOptions<TOptions>().BindCommand<TCommand, TOptions>().Services
            .AddOptions<CommandArgumentMapper<TCommand>>().Services;
    }

    public static IServiceCollection AddRootCommand<TRootCommand>(this IServiceCollection services, string[] args)
    where TRootCommand : RootCommand, IUseCommandBuilder<TRootCommand>
    {
        return services.AddCommand<TRootCommand>().AddSingleton(sp =>
        {
            return sp.GetRequiredService<TRootCommand>().Parse(args);
        });
    }
    public static IServiceCollection AddCommand<TCommand>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        return services.AddSingleton(sp =>
        {
            return TCommand.CommandFactory(sp, GetArgumentMapperRegistration<TCommand>(sp));
        });
    }

    public static IServiceCollection AddCommandWithAsyncHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : Command, IUseCommandBuilder<TCommand>
        where THandler : AsynchronousCommandLineAction
    {
        return services.AddSingleton<THandler>().AddSingleton(sp =>
        {
            return TCommand.CommandFactory(sp, GetArgumentMapperRegistration<TCommand>(sp)).SetAsyncHandler<TCommand, THandler>(sp);
        });
    }

    public static IServiceCollection AddCommandWithHandler<TCommand, THandler>(this IServiceCollection services) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : SynchronousCommandLineAction
    {
        return services.AddSingleton<THandler>().AddSingleton(sp =>
        {
            return TCommand.CommandFactory(sp, GetArgumentMapperRegistration<TCommand>(sp)).SetHandler<TCommand, THandler>(sp);
        });
    }

    public static TCommand SetAsyncHandler<TCommand, THandler>(this TCommand command, IServiceProvider sp) where TCommand : Command, IUseCommandBuilder<TCommand> where THandler : AsynchronousCommandLineAction
    {
        command.SetAction((pr, ct) => sp.GetRequiredService<THandler>().InvokeAsync(pr, ct));
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

using System.CommandLine;
using System.CommandLine.Invocation;

using SystemCommandLine.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ServiceCollectionExtensions
{
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

}

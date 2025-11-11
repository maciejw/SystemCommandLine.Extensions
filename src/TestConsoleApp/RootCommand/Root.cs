using Microsoft.Extensions.DependencyInjection;

using Serilog.Events;

using System.CommandLine;

using SystemCommandLine.Extensions;

using TestConsoleApp.RootCommand.GreetCommand;

namespace TestConsoleApp.RootCommand;

internal class Root : System.CommandLine.RootCommand, IUseCommandBuilder<Root>
{
    public Root(ArgumentMapperRegistration mapperRegistration) : base("Sample ConsoleApp with DI and Serilog")
    {
        this.UseCommandBuilder().WithMapping<LoggerOptions>(mapperRegistration)
            .NewOption(o => o.LogEventLevel).Configure(o =>
            {
                o.Recursive = true;
                o.DefaultValueFactory = _ => LogEventLevel.Information;
            }).AddToCommand();
    }

    public static Root CommandFactory(IServiceProvider sp, ArgumentMapperRegistration mapperRegistration)
    {
        bool subCommandRegistrationVariant = true;
        if (subCommandRegistrationVariant)
        {
            /// Here we add multiple sub-commands at once and let the DI container resolve them.
            /// in this case we dont have to know subcommands in Root class. This can be useful in 
            /// modular applications where we reference ParentCommand in SubCommand projects.
            /// We can also register parent behind an interface that parent and subcommands reference.
            return new Root(mapperRegistration).AddRegisteredSubCommands(sp);
        }
        else
        {
            /// Here we manually add each sub-command manually, resolving them from the DI container 
            /// so they get its dependecies.
            /// In this case maybe we have all commands in one project and we can reference them directly.
            return new Root(mapperRegistration) {
                sp.GetRequiredService<Greet>()
            };
        }
    }
}
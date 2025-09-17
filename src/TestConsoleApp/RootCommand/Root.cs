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
        return new Root(mapperRegistration)
        {
            sp.GetRequiredService<Greet>(),
        };
    }
}

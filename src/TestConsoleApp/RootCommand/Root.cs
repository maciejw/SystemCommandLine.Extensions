using Microsoft.Extensions.DependencyInjection;

using Serilog.Events;

using System.CommandLine;

using SystemCommandLine.Extensions;

using TestConsoleApp.RootCommand.GreetCommand;

namespace TestConsoleApp.RootCommand;


internal class Root : System.CommandLine.RootCommand, IUseCommandBuilder<Root>
{
    public Root(ArgumentMapperRegistration register) : base("Sample ConsoleApp with DI and Serilog")
    {
        this.UsesCommandBuilder().With<LoggerOptions>()
            .NewOption(o => o.LogEventLevel).Configure(o =>
            {
                o.Recursive = true;
                o.DefaultValueFactory = _ => LogEventLevel.Information;
            }).AddToCommand(register);
    }

    public static Root CommandFactory(IServiceProvider sp, ArgumentMapperRegistration register)
    {
        return new Root(register)
        {
            sp.GetRequiredService<Greet>(),
        };
    }
}

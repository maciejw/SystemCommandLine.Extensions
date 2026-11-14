using Serilog.Events;

using System.CommandLine;

using SystemCommandLine.Extensions;

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

    public static void ConfigureRegistration(CommandRegistrationBuilder<Root> builder)
    {
        builder.BindOptions<LoggerOptions>();
    }

    public static Root CommandFactory(IServiceProvider sp, ArgumentMapperRegistration mapperRegistration)
    {
        return new Root(mapperRegistration);
    }
}

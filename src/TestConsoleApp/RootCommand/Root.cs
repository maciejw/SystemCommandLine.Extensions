using Serilog.Events;

using System.CommandLine;

using SystemCommandLine.Extensions;

namespace TestConsoleApp.RootCommand;

internal class Root : System.CommandLine.RootCommand, ICommandDefinition<Root>
{
    public Root(SymbolMapperRegistration mapperRegistration) : base("Sample ConsoleApp with DI and Serilog")
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

    public static Root CreateCommand(IServiceProvider sp, SymbolMapperRegistration mapperRegistration) => new(mapperRegistration);
}

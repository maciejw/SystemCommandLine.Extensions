using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

using SystemCommandLine.Extensions;

namespace TestConsoleApp.RootCommand.GreetCommand;

// Update the README quick start when changing this command or its handler.
public class Greet : Command, ICommandDefinition<Greet>
{
    public Greet(SymbolMapperRegistration mapperRegistration) : base("greet", "Greets a person")
    {
        this.UseCommandBuilder()
            .NewOption<string>("prefix").Configure(o =>
            {
                o.AcceptOnlyFromAmong(StringComparer.OrdinalIgnoreCase, "Sir", "Mr", "Lord");
                o.Description = "An example option without DI mapping";
            }).AddToCommand()
            .WithMapping<GreetOptions>(mapperRegistration)
            .NewOption(x => x.Name).Configure(o =>
            {
                o.DefaultValueFactory = _ => "World";
                o.Description = "Name of the person to greet";
            }).AddToCommand()
            .NewOption(x => x.Times).Configure(o =>
            {
                o.DefaultValueFactory = _ => 1;
                o.Description = "Number of times to greet";
            }).AddToCommand()
            .NewOption(x => x.Shout).Configure(o =>
            {
                o.Description = "Whether to shout the greeting";
            }).AddToCommand();
    }

    public static void ConfigureRegistration(CommandRegistrationBuilder<Greet> builder)
    {
        builder.WithAsyncHandler<GreetHandler>().BindOptions<GreetOptions>();
    }

    public static Greet CreateCommand(IServiceProvider sp, SymbolMapperRegistration mapperRegistration) => new(mapperRegistration);

    private class GreetOptions
    {
        public required string Name { get; set; }
        public int Times { get; set; }
        public bool Shout { get; set; }
    }

    private class GreetHandler(IOptions<GreetOptions> options, ILogger<GreetHandler> logger) : AsynchronousCommandLineAction
    {
        public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            string name = options.Value.Name;
            if (options.Value.Shout)
            {
                name = name.ToUpperInvariant();
            }

            if (parseResult.GetValue<string>("--prefix") is string prefix)
            {
                name = $"{prefix} {name}";
            }

            logger.LogDebug("Greeting {Name} {Times} time(s)", name, options.Value.Times);
            for (int i = 0; i < options.Value.Times; i++)
            {
                parseResult.InvocationConfiguration.Output.WriteLine($"Hello, {name}!");
            }

            return Task.FromResult(0);
        }
    }
}
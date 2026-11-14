using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

using SystemCommandLine.Extensions;

namespace TestConsoleApp.RootCommand.WaveCommand;

internal sealed class Wave : Command, ICommandDefinition<Wave>
{
    private Wave(SymbolMapperRegistration mapperRegistration) : base("wave", "Waves to a person using positional arguments")
    {
        this.UseCommandBuilder()
            .WithMapping<WaveOptions>(mapperRegistration)
            .NewArgument(x => x.Name).Configure(a =>
            {
                a.Description = "Name of the person to wave to";
            }).AddToCommand()
            .NewOption(x => x.Shout).Configure(o =>
            {
                o.Description = "Whether to shout the message";
            }).AddToCommand()
            .CommandBuilder()
            .NewArgument<string>("message").Configure(a =>
            {
                a.Description = "Optional message, read directly from the parse result";
                a.DefaultValueFactory = _ => "Hello";
            }).AddToCommand();
    }

    public static Wave CreateCommand(IServiceProvider sp, SymbolMapperRegistration mapperRegistration) => new(mapperRegistration);

    public static void ConfigureRegistration(CommandRegistrationBuilder<Wave> builder)
        => builder.WithHandler<WaveHandler>().BindOptions<WaveOptions>();

    private sealed class WaveOptions
    {
        public required string Name { get; set; }
        public bool Shout { get; set; }
    }

    private sealed class WaveHandler(IOptions<WaveOptions> options) : SynchronousCommandLineAction
    {
        public override int Invoke(ParseResult parseResult)
        {
            string message = parseResult.GetRequiredValue<string>("message");
            string output = $"{message}, {options.Value.Name}! *waves*";
            if (options.Value.Shout)
            {
                output = output.ToUpperInvariant();
            }

            parseResult.InvocationConfiguration.Output.WriteLine(output);
            return 0;
        }
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

using SystemCommandLine.Extensions;

namespace TestConsoleApp.RootCommand.GreetCommand;

public class Greet : Command, IUseCommandBuilder<Greet>
{
    public Greet(ArgumentMapperRegistration mapperRegistration) : base(nameof(Greet).ToLower(), "Greets a person")
    {
        this.UseCommandBuilder()
            .NewOption<string>("prefix").Configure(o =>
            {
                string[] values = ["Sir", "Mr", "Lord"];
                o.DefaultValueFactory = _ => "Mr";
                o.Validators.Add(r =>
                {
                    string? value = r.GetValue<string>("--prefix");
                    if (!values.Contains(value))
                    {
                        r.AddError($"The value '{value}' is not allowed for --prefix. Valid values are {string.Join(", ", values)}");
                    }
                });
                o.CompletionSources.Add(values);
                o.Description = "An example option without DI mapping";
            }).AddToCommand()

            .WithMapping<GreetOptions>(mapperRegistration)
            .NewOption(x => x.Name).Configure(o =>
            {
                o.Required = true;
                o.Description = "Name of the person to greet";
            }).AddToCommand()
            .NewOption(x => x.Times).Configure(o =>
            {
                o.Description = "Number of times to greet";
                o.DefaultValueFactory = _ => 1;
            }).AddToCommand()
            .NewOption(x => x.Shout).Configure(o =>
            {
                o.Description = "Whether to shout the greeting";
            }).AddToCommand();
    }

    public static Greet CommandFactory(IServiceProvider sp, ArgumentMapperRegistration mapperRegistration)
    {
        return new Greet(mapperRegistration);
    }
}

public class GreetOptions
{
    public required string Name { get; set; }
    public int Times { get; set; }
    public bool Shout { get; set; }
}

public class GreetHandler(IOptions<GreetOptions> options, ILogger<GreetHandler> logger) : AsynchronousCommandLineAction
{
    public async override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        logger.LogDebug("greet");
        logger.LogTrace("greet");
        string name = options.Value.Name;
        if (options.Value.Shout == true)
        {
            name = name.ToUpper();
        }

        string prefix = parseResult.GetRequiredValue<string>("--prefix");

        for (int i = 0; i < options.Value.Times; i++)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Hello, {prefix} {name}!", prefix, name);
            }
        }
        return 0;
    }
}

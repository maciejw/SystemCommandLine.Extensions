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
        this.UseCommandBuilder(mapperRegistration).With<GreetOptions>()
            .NewOption(x => x.Name).Configure(o =>
            {
                o.Description = "Name of the person to greet";
                o.DefaultValueFactory = _ => "World";
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
    public string Name { get; set; } = "World";
    public int Times { get; set; } = 1;
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

        for (int i = 0; i < options.Value.Times; i++)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Hello, {name}!", name);
            }
        }
        return 0;
    }
}

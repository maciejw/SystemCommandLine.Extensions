using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Serilog;

using System.CommandLine;

using TestConsoleApp.RootCommand;
using TestConsoleApp.RootCommand.GreetCommand;
using TestConsoleApp.RootCommand.WaveCommand;

namespace TestConsoleApp;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder();

        hostBuilder.UseConsoleLifetime();
        hostBuilder.UseSerilog((ctx, sp, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration)
            .MinimumLevel.Is(sp.GetRequiredService<IOptions<LoggerOptions>>().Value.LogEventLevel));

        hostBuilder.ConfigureServices((ctx, services) =>
        {
            services.AddRootCommand<Root>(args)
                .WithSubcommand<Greet>()
                .WithSubcommand<Wave>();

            services.AddCommand<Greet>();
            services.AddCommand<Wave>();
        });

        IHost host = hostBuilder.Start();

        IHostApplicationLifetime lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        ParseResult parseResult = host.Services.GetRequiredService<ParseResult>();

        int result = await parseResult.InvokeAsync(parseResult.InvocationConfiguration, lifetime.ApplicationStopping);

        await host.StopAsync(TimeSpan.FromMinutes(1));

        return result;
    }
}

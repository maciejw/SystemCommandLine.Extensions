using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace SystemCommandLine.Extensions.Tests;

public class CommandBuilderTests
{
    private class TestRootCommand : RootCommand, IUseCommandBuilder<TestRootCommand>
    {
        public static TestRootCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration)
        {
            return new TestRootCommand() {
                serviceProvider.GetRequiredService<TestCommand>()
            };
        }
    }

    private class TestCommand : Command, IUseCommandBuilder<TestCommand>
    {
        public TestCommand(ArgumentMapperRegistration mapperRegistration) : base("test", "A test command")
        {
            this.UseCommandBuilder().WithMapping<TestCommandOptions>(mapperRegistration)
                .NewOption(x => x.Name).Configure(o =>
                {
                    o.Description = "Name of the test";
                    o.DefaultValueFactory = _ => "DefaultTest";
                }).AddToCommand()
                .NewOption(x => x.Count).Configure(o =>
                {
                    o.Description = "Count for the test";
                    o.DefaultValueFactory = _ => 1;
                }).AddToCommand().CommandBuilder()
                .NewOption<string>("NotMappedOption").Configure(o =>
                {
                    o.Description = "An option that is not mapped to the options class";
                }).AddToCommand();
        }
        public static TestCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration) => new(mapperRegistration);

        public class TestCommandOptions
        {
            public required string Name { get; set; }
            public int Count { get; set; }
        }
        public class TestHandler(IOptions<TestCommandOptions> options) : SynchronousCommandLineAction
        {
            public override int Invoke(ParseResult parseResult)
            {
                string notMappedOption = parseResult.GetRequiredValue<string>(NameFormatExtensions.ToKebabCase("--", nameof(notMappedOption)));

                parseResult.InvocationConfiguration
                    .Output.WriteLine($"Running test '{options.Value.Name}' {options.Value.Count} times {notMappedOption}");

                parseResult.InvocationConfiguration
                    .Error.WriteLine($"Error message");
                return 42; // Return success code
            }
        }
        public class TestAsyncHandler(IOptions<TestCommandOptions> options) : AsynchronousCommandLineAction
        {
            public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
            {
                string notMappedOption = parseResult.GetRequiredValue<string>(NameFormatExtensions.ToKebabCase("--", nameof(notMappedOption)));
                parseResult.InvocationConfiguration
                    .Output.WriteLine($"Running test '{options.Value.Name}' {options.Value.Count} times {notMappedOption}");

                parseResult.InvocationConfiguration
                    .Error.WriteLine($"Error message");
                return Task.FromResult(42); // Return success code
            }
        }
    }

    [Fact]
    public void Should_run_synchronous_handler()
    {
        IServiceCollection services = new ServiceCollection()
            .AddRootCommand<TestRootCommand>("test --name MyTest --count 5 --not-mapped-option !!!".Split(" "))
            .AddCommandWithHandler<TestCommand, TestCommand.TestHandler>()
            .AddBoundToCommandOptions<TestCommand, TestCommand.TestCommandOptions>()
            ;
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        ParseResult parserResult = serviceProvider.GetRequiredService<ParseResult>();

        InvocationConfiguration configuration = new()
        {
            Error = new StringWriter(),
            Output = new StringWriter(),
        };
        int result = parserResult.Invoke(configuration);
        Assert.Matches(@"Error message", configuration.Error.ToString());
        Assert.Equal(42, result);
        Assert.Matches(@"Running test 'MyTest' 5 times !!!", configuration.Output.ToString());
    }

    [Fact]
    public async Task Should_run_asynchronous_handler()
    {
        IServiceCollection services = new ServiceCollection()
            .AddRootCommand<TestRootCommand>("test --name MyTest --count 5 --not-mapped-option !!!".Split(" "))
            .AddCommandWithAsyncHandler<TestCommand, TestCommand.TestAsyncHandler>()
            .AddBoundToCommandOptions<TestCommand, TestCommand.TestCommandOptions>()
            ;
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        ParseResult parserResult = serviceProvider.GetRequiredService<ParseResult>();

        InvocationConfiguration configuration = new()
        {
            Error = new StringWriter(),
            Output = new StringWriter(),
        };
        int result = await parserResult.InvokeAsync(configuration, TestContext.Current.CancellationToken);

        Assert.Matches(@"Error message", configuration.Error.ToString());
        Assert.Equal(42, result);
        Assert.Matches(@"Running test 'MyTest' 5 times !!!", configuration.Output.ToString());
    }
}

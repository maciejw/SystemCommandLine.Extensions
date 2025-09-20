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
                    o.Description = "Name with option default";
                    o.DefaultValueFactory = _ => "OptionDefault";
                }).AddToCommand()
                .NewOption(x => x.NameWithDefault).Configure(o =>
                {
                    o.Description = "Name with property default";
                }).AddToCommand()
                .NewOption(x => x.Count).Configure(o =>
                {
                    o.Description = "Count for the test";
                    o.DefaultValueFactory = _ => 1;
                }).AddToCommand().CommandBuilder()
                .NewOption<string>("NotMappedOption").Configure(o =>
                {
                    o.Description = "An option that is not mapped to the options class";
                    o.DefaultValueFactory = _ => "!";
                }).AddToCommand();
        }
        public static TestCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration) => new(mapperRegistration);

        public class TestCommandOptions
        {
            public required string Name { get; set; }
            public string NameWithDefault { get; set; } = "PropertyDefault";
            public int Count { get; set; }
        }
        public class TestHandler(IOptions<TestCommandOptions> options) : SynchronousCommandLineAction
        {
            public override int Invoke(ParseResult parseResult)
            {
                return Handler("sync", parseResult, options);
            }
        }
        public class TestAsyncHandler(IOptions<TestCommandOptions> options) : AsynchronousCommandLineAction
        {
            public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
            {
                await Task.Yield();
                return Handler("async", parseResult, options);
            }
        }

        private static int Handler(string handlerType, ParseResult parseResult, IOptions<TestCommandOptions> options)
        {
            string notMappedOption = parseResult.GetRequiredValue<string>(NameFormatExtensions.ToKebabCase("--", nameof(notMappedOption)));

            parseResult.InvocationConfiguration
                .Output.WriteLine($"Running {handlerType} test '{options.Value.Name}' '{options.Value.NameWithDefault}' {options.Value.Count} times {notMappedOption}");

            parseResult.InvocationConfiguration
                .Error.WriteLine($"Error message");
            return 42; // Return success code
        }
    }


    private static ServiceProvider GetServiceProvider(string args, Func<IServiceCollection, IServiceCollection> setup)
    {
        IServiceCollection services = new ServiceCollection()
            .AddRootCommand<TestRootCommand>(args.Split(" "))
            .AddBoundToCommandOptions<TestCommand, TestCommand.TestCommandOptions>();
        return setup(services).BuildServiceProvider();
    }
    private static ServiceProvider GetServiceProviderWithAsyncHandler(string args)
    {
        return GetServiceProvider(args, services => services
                .AddCommandWithAsyncHandler<TestCommand, TestCommand.TestAsyncHandler>());
    }

    private static ServiceProvider GetServiceProviderWithHandler(string args)
    {
        return GetServiceProvider(args, services => services
                .AddCommandWithHandler<TestCommand, TestCommand.TestHandler>());
    }
    [Fact]
    public void Should_run_synchronous_handler()
    {
        using var serviceProvider = GetServiceProviderWithHandler("test --name MyTest --count 5 --not-mapped-option !!!");
        ParseResult parserResult = serviceProvider.GetRequiredService<ParseResult>();

        InvocationConfiguration configuration = new()
        {
            Error = new StringWriter(),
            Output = new StringWriter(),
        };
        int result = parserResult.Invoke(configuration);
        Assert.Matches(@"Error message", configuration.Error.ToString());
        Assert.Equal(42, result);
        Assert.Matches(@"Running sync test 'MyTest' 'PropertyDefault' 5 times !!!", configuration.Output.ToString());
    }

    [Fact]
    public async Task Should_run_asynchronous_handler()
    {
        using ServiceProvider serviceProvider = GetServiceProviderWithAsyncHandler("test --name MyTest --count 5 --not-mapped-option !!!");

        ParseResult parserResult = serviceProvider.GetRequiredService<ParseResult>();

        InvocationConfiguration configuration = new()
        {
            Error = new StringWriter(),
            Output = new StringWriter(),
        };
        int result = await parserResult.InvokeAsync(configuration, TestContext.Current.CancellationToken);

        Assert.Matches(@"Error message", configuration.Error.ToString());
        Assert.Equal(42, result);
        Assert.Matches(@"Running async test 'MyTest' 'PropertyDefault' 5 times !!!", configuration.Output.ToString());
    }

    [Fact]
    public void Should_allow_option_and_property_default_values()
    {
        using ServiceProvider serviceProvider = GetServiceProviderWithHandler("test");

        ParseResult parserResult = serviceProvider.GetRequiredService<ParseResult>();

        InvocationConfiguration configuration = new()
        {
            Error = new StringWriter(),
            Output = new StringWriter(),
        };
        int result = parserResult.Invoke(configuration);

        Assert.Equal(42, result);
        Assert.Matches(@"Running sync test 'OptionDefault' 'PropertyDefault' 1 times !", configuration.Output.ToString());
    }
}

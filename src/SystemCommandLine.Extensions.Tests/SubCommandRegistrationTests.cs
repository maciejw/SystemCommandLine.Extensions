using Microsoft.Extensions.DependencyInjection;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace SystemCommandLine.Extensions.Tests;

public class SubCommandRegistrationTests
{
    private class TestRootCommandThatUsesAddRegisteredSubCommands : RootCommand, IUseCommandBuilder<TestRootCommandThatUsesAddRegisteredSubCommands>
    {
        public static TestRootCommandThatUsesAddRegisteredSubCommands CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration)
        {
            return new TestRootCommandThatUsesAddRegisteredSubCommands().AddRegisteredSubCommands(serviceProvider);
        }
    }
    private class TestRootCommandThatResolvesSubCommandsManually : RootCommand, IUseCommandBuilder<TestRootCommandThatResolvesSubCommandsManually>
    {
        public static TestRootCommandThatResolvesSubCommandsManually CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration)
        {
            return new TestRootCommandThatResolvesSubCommandsManually() {
               serviceProvider.GetRequiredService<TestSubCommand>(),
            };
        }
    }

    private class TestSubCommand : Command, IUseCommandBuilder<TestSubCommand>
    {
        public TestSubCommand() : base("test", "A test command")
        {
        }

        public static TestSubCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration)
        {
            return new TestSubCommand();
        }
    }
    private class TestSubCommandMarkedWithISubCommandOf : Command, IUseCommandBuilder<TestSubCommandMarkedWithISubCommandOf>, ISubCommandOf<TestRootCommandThatUsesAddRegisteredSubCommands>
    {
        public TestSubCommandMarkedWithISubCommandOf() : base("test", "A test command")
        {
        }
        public static TestSubCommandMarkedWithISubCommandOf CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration) => new();
    }
    public class TestHandler() : SynchronousCommandLineAction
    {
        public override int Invoke(ParseResult parseResult)
        {
            return 42;
        }
    }
    public class TestAsyncHandler() : AsynchronousCommandLineAction
    {
        public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return 42;
        }
    }

    [Fact]
    public void Should_register_subcommand_using_ISubCommandOf()
    {
        var services = new ServiceCollection();
        services.AddRootCommand<TestRootCommandThatUsesAddRegisteredSubCommands>(["test"]);

        services.AddCommand<TestSubCommandMarkedWithISubCommandOf>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var parsedResult = serviceProvider.GetRequiredService<ParseResult>();
        var result = parsedResult.Invoke();

        Assert.Equal(0, result);
    }

    [Fact]
    public void Should_register_subcommand_manually()
    {
        var services = new ServiceCollection();
        services.AddRootCommand<TestRootCommandThatResolvesSubCommandsManually>(["test"]);
        services.AddCommand<TestSubCommand>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var parsedResult = serviceProvider.GetRequiredService<ParseResult>();
        var result = parsedResult.Invoke();

        Assert.Equal(0, result);
    }
}

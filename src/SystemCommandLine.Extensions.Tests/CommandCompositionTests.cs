using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

namespace SystemCommandLine.Extensions.Tests;

public class CommandCompositionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void External_composition_supports_nested_commands_and_preserves_factory_children(bool childFirst)
    {
        var services = new ServiceCollection();
        if (childFirst)
        {
            RegisterLeaf(services);
        }

        services.AddRootCommand<TestRoot>(["group", "leaf", "--value", "42"])
            .AddSubCommand<GroupCommand>();
        services.AddCommand<GroupCommand>().AddSubCommand<LeafCommand>();

        if (!childFirst)
        {
            RegisterLeaf(services);
        }

        using var provider = services.BuildServiceProvider();
        var parsed = provider.GetRequiredService<ParseResult>();
        var root = provider.GetRequiredService<TestRoot>();
        var group = provider.GetRequiredService<GroupCommand>();
        var leaf = provider.GetRequiredService<LeafCommand>();

        Assert.Empty(parsed.Errors);
        Assert.Same(group, Assert.Single(root.Subcommands));
        Assert.Equal(new[] { "built-in", "leaf" }, group.Subcommands.Select(c => c.Name));
        Assert.Same(leaf, group.Subcommands[1]);
        Assert.Equal(42, parsed.Invoke());
        Assert.Equal(13, root.Parse("group built-in").Invoke());
    }

    [Theory]
    [InlineData(false, 17)]
    [InlineData(true, 23)]
    public async Task Commands_with_handlers_can_be_configured_as_parents(bool asynchronous, int exitCode)
    {
        var services = new ServiceCollection();
        services.AddRootCommand<TestRoot>(["group"]).AddSubCommand<GroupCommand>();
        var groupBuilder = services.AddCommand<GroupCommand>().AddSubCommand<DisposableCommand>();
        if (asynchronous)
        {
            groupBuilder.WithAsyncHandler<AsyncGroupHandler>();
        }
        else
        {
            groupBuilder.WithHandler<GroupHandler>();
        }

        services.AddCommand<DisposableCommand>();
        using var provider = services.BuildServiceProvider();
        var parsed = provider.GetRequiredService<ParseResult>();
        var root = provider.GetRequiredService<TestRoot>();
        Assert.Empty(parsed.Errors);
        Assert.Equal(exitCode, await parsed.InvokeAsync(cancellationToken: TestContext.Current.CancellationToken));
        var childResult = root.Parse("group disposable");
        Assert.Empty(childResult.Errors);
        Assert.Equal(31, await childResult.InvokeAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Child_declarations_preserve_order_and_do_not_duplicate_singletons()
    {
        var services = new ServiceCollection();
        services.AddRootCommand<TestRoot>([])
            .AddSubCommand<GroupCommand>()
            .AddSubCommand<DisposableCommand>()
            .AddSubCommand<GroupCommand>();
        services.AddCommand<GroupCommand>();
        services.AddCommand<DisposableCommand>();

        using var provider = services.BuildServiceProvider();
        var root = provider.GetRequiredService<TestRoot>();
        Assert.Same(root, provider.GetRequiredService<TestRoot>());
        Assert.Same(root, provider.GetRequiredService<ParseResult>().CommandResult.Command);
        Assert.Equal("factory", root.Description);
        Assert.Equal(new[] { "group", "disposable" }, root.Subcommands.Select(child => child.Name));
        Assert.Same(provider.GetRequiredService<GroupCommand>(), root.Subcommands[0]);
        Assert.Same(provider.GetRequiredService<DisposableCommand>(), root.Subcommands[1]);
    }

    [Fact]
    public void Externally_attached_command_is_disposed_once_by_its_registration()
    {
        var services = new ServiceCollection();
        services.AddRootCommand<TestRoot>([]).AddSubCommand<DisposableCommand>();
        services.AddCommand<DisposableCommand>();

        DisposableCommand child;
        using (var provider = services.BuildServiceProvider())
        {
            var root = provider.GetRequiredService<TestRoot>();
            child = provider.GetRequiredService<DisposableCommand>();
            Assert.Same(child, Assert.Single(root.Subcommands));
            Assert.Equal(0, child.DisposeCount);
        }

        Assert.Equal(1, child.DisposeCount);
    }

    [Fact]
    public void Missing_child_registration_fails_when_composing_the_parent()
    {
        var services = new ServiceCollection();
        services.AddRootCommand<TestRoot>([]).AddSubCommand<DisposableCommand>();

        using var provider = services.BuildServiceProvider();
        var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<TestRoot>());
        Assert.Contains(nameof(DisposableCommand), exception.Message);
    }

    [Fact]
    public void Command_cannot_be_its_own_child()
    {
        var builder = new ServiceCollection().AddCommand<GroupCommand>();
        Assert.Throws<InvalidOperationException>(() => builder.AddSubCommand<GroupCommand>());
    }

    [Fact]
    public void Builder_keeps_parent_context_and_returns_the_original_services()
    {
        var services = new ServiceCollection();
        var rootBuilder = services.AddRootCommand<TestRoot>([]);
        Assert.Same(rootBuilder, rootBuilder.AddSubCommand<DisposableCommand>().WithHandler<GroupHandler>());
        Assert.Same(services, rootBuilder.Services);
        rootBuilder.Services.AddCommand<DisposableCommand>();

        using var provider = services.BuildServiceProvider();
        var root = provider.GetRequiredService<TestRoot>();
        Assert.Equal(17, provider.GetRequiredService<ParseResult>().Invoke());
        Assert.Equal(31, root.Parse("disposable").Invoke());
    }

    [Fact]
    public void Factory_attached_child_is_not_attached_again()
    {
        var services = new ServiceCollection();
        services.AddRootCommand<FactoryRoot>([]).AddSubCommand<DisposableCommand>();
        services.AddCommand<DisposableCommand>();

        using var provider = services.BuildServiceProvider();
        var root = provider.GetRequiredService<FactoryRoot>();
        Assert.Same(provider.GetRequiredService<DisposableCommand>(), Assert.Single(root.Subcommands));
        Assert.Equal(31, root.Parse("disposable").Invoke());
    }

    [Fact]
    public void Repeated_options_binding_maps_values_once()
    {
        var services = new ServiceCollection();
        services.AddRootCommand<TestRoot>(["leaf", "--value", "42"]).AddSubCommand<LeafCommand>();
        var builder = services.AddCommand<LeafCommand>().WithHandler<LeafHandler>();
        Assert.Same(builder, builder.BindOptions<LeafOptions>().BindOptions<LeafOptions>());

        using var provider = services.BuildServiceProvider();
        Assert.Equal(42, provider.GetRequiredService<ParseResult>().Invoke());
        Assert.Equal(1, provider.GetRequiredService<IOptions<LeafOptions>>().Value.BindingCount);
    }

    [Theory]
    [InlineData(false, 17)]
    [InlineData(true, 23)]
    public async Task Last_handler_selection_wins(bool asynchronous, int exitCode)
    {
        var services = new ServiceCollection();
        var builder = services.AddRootCommand<TestRoot>([]);
        if (asynchronous)
        {
            builder.WithHandler<GroupHandler>().WithAsyncHandler<AsyncGroupHandler>();
        }
        else
        {
            builder.WithAsyncHandler<AsyncGroupHandler>().WithHandler<GroupHandler>();
        }

        using var provider = services.BuildServiceProvider();
        Assert.Equal(exitCode, await provider.GetRequiredService<ParseResult>()
            .InvokeAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Command_registration_hook_supplies_defaults_without_eager_construction()
    {
        var services = new ServiceCollection();
        services.AddRootCommand<TestRoot>(["self-registered", "--value", "42"])
            .AddSubCommand<SelfRegisteredCommand>();
        services.AddCommand<SelfRegisteredCommand>().BindOptions<LeafOptions>();

        // The hook runs during registration, but its command and handler remain lazy.
        var descriptor = Assert.Single(services, service => service.ServiceType == typeof(RegistrationProbe));
        var probe = Assert.IsType<RegistrationProbe>(descriptor.ImplementationInstance);
        Assert.Equal(0, probe.CommandCreations);
        Assert.Equal(0, probe.HandlerCreations);

        using var provider = services.BuildServiceProvider();
        var parsed = provider.GetRequiredService<ParseResult>();
        Assert.Empty(parsed.Errors);
        Assert.Equal(1, probe.CommandCreations);
        Assert.Equal(0, probe.HandlerCreations);
        Assert.Equal(42, parsed.Invoke());
        Assert.Equal(42, parsed.Invoke());
        Assert.Equal(1, probe.CommandCreations);
        Assert.Equal(1, probe.HandlerCreations);
        Assert.Equal(1, provider.GetRequiredService<IOptions<LeafOptions>>().Value.BindingCount);
        Assert.Single(services, service => service.ServiceType == typeof(RegistrationProbe));
    }

    [Fact]
    public void Root_registration_hook_supplies_handler_and_bound_options()
    {
        var services = new ServiceCollection();
        services.AddRootCommand<SelfRegisteredRoot>(["--value", "53"]);

        using var provider = services.BuildServiceProvider();
        var parsed = provider.GetRequiredService<ParseResult>();
        Assert.Empty(parsed.Errors);
        Assert.Equal(53, parsed.Invoke());
        Assert.Equal(1, provider.GetRequiredService<IOptions<LeafOptions>>().Value.BindingCount);
    }

    [Theory]
    [InlineData(false, 17)]
    [InlineData(true, 23)]
    public async Task Application_can_override_the_command_default_handler(bool asynchronous, int exitCode)
    {
        var services = new ServiceCollection();
        services.AddRootCommand<TestRoot>(["self-registered", "--value", "42"])
            .AddSubCommand<SelfRegisteredCommand>();
        var builder = services.AddCommand<SelfRegisteredCommand>();
        if (asynchronous)
        {
            builder.WithAsyncHandler<AsyncGroupHandler>();
        }
        else
        {
            builder.WithHandler<GroupHandler>();
        }

        using var provider = services.BuildServiceProvider();
        var parsed = provider.GetRequiredService<ParseResult>();
        Assert.Empty(parsed.Errors);
        Assert.Equal(exitCode, await parsed.InvokeAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(0, provider.GetRequiredService<RegistrationProbe>().HandlerCreations);
    }

    private static void RegisterLeaf(IServiceCollection services)
    {
        services.AddCommand<LeafCommand>().WithHandler<LeafHandler>().BindOptions<LeafOptions>();
    }

    private sealed class TestRoot() : RootCommand("factory"), IUseCommandBuilder<TestRoot>
    {
        public static TestRoot CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration) => new();
    }

    private sealed class FactoryRoot : RootCommand, IUseCommandBuilder<FactoryRoot>
    {
        public static FactoryRoot CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration) =>
            new() { serviceProvider.GetRequiredService<DisposableCommand>() };
    }

    private sealed class GroupCommand : Command, IUseCommandBuilder<GroupCommand>
    {
        private GroupCommand() : base("group")
        {
            var child = new Command("built-in");
            child.SetAction(_ => 13);
            Add(child);
        }

        public static GroupCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration) => new();
    }

    private sealed class LeafCommand : Command, IUseCommandBuilder<LeafCommand>
    {
        private LeafCommand(ArgumentMapperRegistration mapperRegistration) : base("leaf")
        {
            this.UseCommandBuilder().WithMapping<LeafOptions>(mapperRegistration)
                .NewOption(options => options.Value).AddToCommand();
        }

        public static LeafCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration) => new(mapperRegistration);
    }

    private sealed class LeafOptions
    {
        private int value;
        public int BindingCount { get; private set; }
        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                BindingCount++;
            }
        }
    }

    private sealed class LeafHandler(IOptions<LeafOptions> options) : SynchronousCommandLineAction
    {
        public override int Invoke(ParseResult parseResult) => options.Value.Value;
    }

    private sealed class GroupHandler : SynchronousCommandLineAction
    {
        public override int Invoke(ParseResult parseResult) => 17;
    }

    private sealed class AsyncGroupHandler : AsynchronousCommandLineAction
    {
        public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken) => Task.FromResult(23);
    }

    private sealed class RegistrationProbe
    {
        public int CommandCreations { get; set; }
        public int HandlerCreations { get; set; }
    }

    private sealed class SelfRegisteredCommand : Command, IUseCommandBuilder<SelfRegisteredCommand>
    {
        private SelfRegisteredCommand(ArgumentMapperRegistration mapperRegistration) : base("self-registered")
        {
            this.UseCommandBuilder().WithMapping<LeafOptions>(mapperRegistration)
                .NewOption(options => options.Value).AddToCommand();
        }

        public static void ConfigureRegistration(CommandRegistrationBuilder<SelfRegisteredCommand> builder)
        {
            builder.WithHandler<SelfRegisteredHandler>().BindOptions<LeafOptions>()
                .Services.AddSingleton(new RegistrationProbe());
        }

        public static SelfRegisteredCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration)
        {
            serviceProvider.GetRequiredService<RegistrationProbe>().CommandCreations++;
            return new(mapperRegistration);
        }
    }

    private sealed class SelfRegisteredHandler : SynchronousCommandLineAction
    {
        private readonly IOptions<LeafOptions> options;

        public SelfRegisteredHandler(IOptions<LeafOptions> options, RegistrationProbe probe)
        {
            this.options = options;
            probe.HandlerCreations++;
        }

        public override int Invoke(ParseResult parseResult) => options.Value.Value;
    }

    private sealed class SelfRegisteredRoot : RootCommand, IUseCommandBuilder<SelfRegisteredRoot>
    {
        private SelfRegisteredRoot(ArgumentMapperRegistration mapperRegistration)
        {
            this.UseCommandBuilder().WithMapping<LeafOptions>(mapperRegistration)
                .NewOption(options => options.Value).AddToCommand();
        }

        public static void ConfigureRegistration(CommandRegistrationBuilder<SelfRegisteredRoot> builder) =>
            builder.WithHandler<LeafHandler>().BindOptions<LeafOptions>();

        public static SelfRegisteredRoot CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration) =>
            new(mapperRegistration);
    }

    private sealed class DisposableCommand : Command, IUseCommandBuilder<DisposableCommand>, IDisposable
    {
        private DisposableCommand() : base("disposable") => SetAction(_ => 31);

        public int DisposeCount { get; private set; }

        public static DisposableCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration) => new();

        public void Dispose() => DisposeCount++;
    }
}

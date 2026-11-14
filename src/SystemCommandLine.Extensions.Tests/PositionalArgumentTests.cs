using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;

namespace SystemCommandLine.Extensions.Tests;

public class PositionalArgumentTests
{
    private sealed class Inputs
    {
        public string Name { get; set; } = "";
        public bool Shout { get; set; }
        public string[] Files { get; set; } = [];
    }

    private sealed class Root : RootCommand, IUseCommandBuilder<Root>
    {
        public static void ConfigureRegistration(CommandRegistrationBuilder<Root> builder)
            => builder.BindOptions<Inputs>();

        public static Root CommandFactory(IServiceProvider services, ArgumentMapperRegistration register)
        {
            var command = new Root();
            command.UseCommandBuilder().WithMapping<Inputs>(register)
                .NewArgument(x => x.Name).Configure(a => a.Description = "Person to greet").AddToCommand()
                .NewOption(x => x.Shout).AddToCommand().CommandBuilder()
                .NewArgument<int>("RepeatCount").Configure(a => a.DefaultValueFactory = _ => 2).AddToCommand()
                .WithMapping<Inputs>(register)
                .NewArgument(x => x.Files).Configure(a => a.Arity = ArgumentArity.ZeroOrMore).AddToCommand();
            return command;
        }
    }

    private static ServiceProvider Create(params string[] args)
        => new ServiceCollection().AddRootCommand<Root>(args).Services.BuildServiceProvider();

    [Fact]
    public void Maps_arguments_in_order_alongside_options()
    {
        using var services = Create("Jane Doe", "3", "first.txt", "second.txt", "--shout");
        var result = services.GetRequiredService<ParseResult>();
        Assert.Empty(result.Errors);
        var inputs = services.GetRequiredService<IOptions<Inputs>>().Value;
        Assert.Equal("Jane Doe", inputs.Name);
        Assert.True(inputs.Shout);
        Assert.Equal(new[] { "first.txt", "second.txt" }, inputs.Files);
        Assert.Equal(3, result.GetValue<int>("repeat-count"));
        Assert.Equal(new[] { "name", "repeat-count", "files" },
            services.GetRequiredService<Root>().Arguments.Select(a => a.Name));
    }

    [Fact]
    public void Uses_argument_default_and_allows_empty_variadic_argument()
    {
        using var services = Create("Jane");
        var result = services.GetRequiredService<ParseResult>();
        Assert.Empty(result.Errors);
        Assert.Equal(2, result.GetValue<int>("repeat-count"));
        var inputs = services.GetRequiredService<IOptions<Inputs>>().Value;
        Assert.Equal("Jane", inputs.Name);
        Assert.Empty(inputs.Files);
        Assert.False(inputs.Shout);
    }

    [Theory]
    [InlineData()]
    [InlineData("Jane", "invalid-number")]
    public void Reports_missing_or_invalid_arguments(params string[] args)
    {
        using var services = Create(args);
        Assert.NotEmpty(services.GetRequiredService<ParseResult>().Errors);
    }

    [Fact]
    public void Maps_configured_argument_default()
    {
        var command = new Root();
        var mappers = new List<ArgumentMapper>();
        command.UseCommandBuilder().WithMapping<Inputs>(mappers.Add)
            .NewArgument(x => x.Name).Configure(a => a.DefaultValueFactory = _ => "World").AddToCommand();
        var result = command.Parse([]);
        Assert.Empty(result.Errors);
        var inputs = new Inputs();
        foreach (var mapper in mappers)
            mapper(result, inputs);
        Assert.Equal("World", inputs.Name);
    }

}

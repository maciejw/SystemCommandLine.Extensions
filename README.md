## SystemCommandLine.Extensions

[![Build](https://github.com/maciejw/SystemCommandLine.Extensions/actions/workflows/ci-release.yml/badge.svg?branch=master)](https://github.com/maciejw/SystemCommandLine.Extensions/actions/workflows/ci-release.yml)
[![NuGet](https://img.shields.io/nuget/v/SystemCommandLine.Extensions.svg)](https://www.nuget.org/packages/SystemCommandLine.Extensions)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SystemCommandLine.Extensions.svg)](https://www.nuget.org/packages/SystemCommandLine.Extensions)

Small, pragmatic helpers for building command-line apps with
[System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/) and the
Microsoft.Extensions hosting/DI stack.

- Fluent builders to define options from a typed options class using expressions
- Automatic mapping from parsed command-line values to your options (IOptions<T>)
- DI-first wiring for commands and handlers
- Host-friendly RootCommand setup and ParseResult lifetime integration
- Kebab-case option names by convention (e.g., MyProperty -> --my-property)

### Install

From a `net10.0` console project, install the library and the hosting package
used by `Host.CreateDefaultBuilder` in the quick start.

```pwsh
dotnet add package SystemCommandLine.Extensions --version 3.0.0
dotnet add package Microsoft.Extensions.Hosting --version 10.0.15
```

### Quick start

This example defines a `greet` command with `--name` and `--times` options.
For the underlying command-line conventions, see Microsoft's
[guide to commands, options, and arguments](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax).

Create [Greet.cs](src/TestConsoleApp/RootCommand/GreetCommand/Greet.cs) in your
project directory. This is the same command the sample app runs. Its factory
defines the options, and its registration binds those options and selects the
handler. The handler writes greetings to the command output and logs a diagnostic
at debug level.

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.CommandLine;
using System.CommandLine.Invocation;

using SystemCommandLine.Extensions;

namespace TestConsoleApp.RootCommand.GreetCommand;

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
```

Replace `Program.cs` with the code below. It uses the default host's logging
and DI services and registers just the command used in this walkthrough.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using SystemCommandLine.Extensions;
using TestConsoleApp.RootCommand.GreetCommand;

using var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddRootCommand<Root>(args).WithSubcommand<Greet>();
        services.AddCommand<Greet>();
    })
    .Build();

return await host.Services.GetRequiredService<ParseResult>().InvokeAsync();

internal class Root : RootCommand, ICommandDefinition<Root>
{
    public static Root CreateCommand(IServiceProvider serviceProvider, SymbolMapperRegistration mapperRegistration)
        => new();
}
```

Build the project and run the commands from the project directory.

```pwsh
dotnet build
dotnet run -- --help
dotnet run -- greet
dotnet run -- greet --name Ada --times 2
```

The help lists `greet`. The first `greet` invocation prints `Hello, World!` once,
and the second prints `Hello, Ada!` twice.

The [sample app](src/TestConsoleApp/Program.cs) registers both `greet` and `wave`
on one root command and uses Serilog. To see the greeting diagnostic, run this
command from the repository root.

```pwsh
dotnet run --project src/TestConsoleApp -- greet --name Ada --log-event-level Debug
```

### Composing commands

A command can attach children inside its `CreateCommand` method, using ordinary
`Command.Add` or a collection initializer:

```csharp
return new Root()
{
    serviceProvider.GetRequiredService<Greet>()
};
```

To compose commands outside their definitions, use the registration builder
returned by `AddRootCommand<TCommand>` or `AddCommand<TCommand>`:

```csharp
services.AddRootCommand<Root>(args)
    .WithSubcommand<Greet>();
services.AddCommand<Greet>();
```

In this variant, the root factory simply returns `new Root()`;
neither command needs to declare the relationship. Keep any required root options
bindings in the root's `ConfigureRegistration`, for example
`builder.BindOptions<LoggerOptions>()`. See `src/TestConsoleApp` for a complete example.

`AddCommand` and `AddRootCommand` invoke `ConfigureRegistration` once per
registration call, before returning the builder. The hook configures services;
it does not construct the command or handler. Its default interface implementation
does nothing, so existing command classes can omit it and applications can still
register handlers and options explicitly with the builder.

Application-level handler choices override the command's defaults:

```csharp
services.AddCommand<Greet>().WithAsyncHandler<CustomGreetHandler>();
```

Use the command's hook for its own defaults and application startup for hierarchy
composition. Both use the same registration builder; the hook's choices are applied
first, followed by any application-level builder calls.

The builder offers a small set of registration operations:

| Member | Purpose |
| --- | --- |
| `WithHandler<THandler>()` | Register and select a synchronous handler |
| `WithAsyncHandler<THandler>()` | Register and select an asynchronous handler |
| `BindOptions<TOptions>()` | Bind parsed values to the options type mapped by the command |
| `WithSubcommand<TChild>()` | Attach a separately registered child to this command |
| `Services` | Return to the original service collection |

Every method returns the same command's builder. In particular, `WithSubcommand`
does not switch to the child or register it implicitly. The child supplies its
defaults when separately registered with `AddCommand`. Non-root commands use the same builder, so
the pattern works at every level of a hierarchy. The builder exposes neither a
command instance nor an arbitrary configuration callback; option definitions
remain in the command class.

Register each command once and finish configuring its builder before building the
service provider. Parent and child registrations can appear in either order.
Commands are singletons; children are resolved and attached after the parent's
factory and handler setup, before parsing. Factory-defined children are preserved,
and the same child instance is not attached twice. Repeating `WithSubcommand` for
the same child type or `BindOptions` for the same options type on a builder is
harmless. The last `WithHandler` or `WithAsyncHandler` call selects the handler.
Existing handler service registrations are respected.

Keep relationships acyclic: do not resolve a parent while constructing its child.
Missing child service registrations fail when the parent is first resolved.

Use `.Services` to continue ordinary service registration:

```csharp
services.AddCommand<Greet>()
    .WithAsyncHandler<GreetHandler>()
    .BindOptions<GreetOptions>()
    .Services
    .AddSingleton<MyService>();
```

### Migrating registration code

This is a breaking API change: `AddRootCommand` and `AddCommand` now return
`CommandRegistrationBuilder<TCommand>` instead of `IServiceCollection`. Existing
consumers must migrate and recompile. Append `.Services` when continuing a chain
of service registrations or returning the service collection.

The standalone handler/options registration helpers are replaced by builder methods:

| Previous registration | Builder registration |
| --- | --- |
| `services.AddCommandWithHandler<C, H>()` | `services.AddCommand<C>().WithHandler<H>()` |
| `services.AddCommandWithAsyncHandler<C, H>()` | `services.AddCommand<C>().WithAsyncHandler<H>()` |
| `services.AddBoundToCommandOptions<C, O>()` | `.BindOptions<O>()` on the builder for `C` |

In v3, replace `IUseCommandBuilder<TCommand>` with `ICommandDefinition<TCommand>`,
rename `CommandFactory` to `CreateCommand`, and change the mapper parameter type
from `ArgumentMapperRegistration` to `SymbolMapperRegistration`.
Fluent option definitions and the command-level `SetHandler` and `SetAsyncHandler`
methods retain their existing contracts.

`ICommandDefinition<TCommand>` requires
`where TCommand : Command, ICommandDefinition<TCommand>` so the hook can accept
the typed registration builder. Conventional command implementations already
satisfy this constraint; generic wrappers that refer to the interface may need
the additional constraint. `ConfigureRegistration` is optional and does not need
to be added to existing commands.

### Why use this?

System.CommandLine is powerful but low-level. This library smooths out the most
common patterns in real apps:

- Define options once on a POCO and keep them strongly typed end-to-end
- Avoid repetitive glue when mapping parsed values into `IOptions<T>`
- Compose commands and handlers via DI in a host-friendly way

### Positional arguments

Positional arguments can be defined alongside options:

```csharp
command.UseCommandBuilder()
    .WithMapping<GreetOptions>(mapperRegistration)
    .NewArgument(x => x.Name)
        .Configure(a => a.Description = "Person to greet")
        .AddToCommand()
    .NewOption(x => x.Shout)
        .AddToCommand();
```

For an argument without property mapping, use `NewArgument<string>("name")`.
Arguments are parsed in the order they are attached. Use `Configure` to set
arity, defaults, and validation. Mapped arguments use the same
`BindOptions<T>()` registration as mapped options.
Existing option builder types and mapper delegates retain their names and signatures.
Custom implementations of the command builder interfaces must implement the new
`NewArgument` method; missing implementations are reported at compile time.

The sample app includes `wave <name> [<message>] [--shout]`: `name` is mapped
to its private options type, while `message` is read directly from the parse
result and defaults to `Hello`. The `Wave` launch profile runs this example.

```sh
dotnet run --project src/TestConsoleApp --no-launch-profile -- wave John
dotnet run --project src/TestConsoleApp --no-launch-profile -- wave "Jane Doe" "Good morning" --shout
```

### Requirements

- .NET 10.0+
- System.CommandLine 3.0.0

To build this repository, use the .NET 11 SDK or newer and install the .NET 10
runtime to run the tests and sample application. The library targets .NET 10.
The repository's `global.json` selects Microsoft.Testing.Platform for `dotnet test`.

```sh
dotnet restore SystemCommandLine.Extensions.slnx
dotnet build SystemCommandLine.Extensions.slnx --configuration Release --no-restore
dotnet test --solution SystemCommandLine.Extensions.slnx --configuration Release --no-build
```

### Native AOT compatibility

The library enables `IsAotCompatible`.
Mapped option types preserve public properties, and DI handler types preserve
public constructors. Generic wrappers around `WithMapping<TOptions>`,
`WithHandler<THandler>`, or `WithAsyncHandler<THandler>`, and implementations of
`ICommandBuilder<TCommand>`, must propagate the corresponding
`DynamicallyAccessedMembers` annotations.

`TestConsoleApp` supports opt-in Native AOT publishing.

Windows publishing requires the Visual Studio Desktop development with C++
workload and Windows SDK;
see the [Native AOT prerequisites](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

```sh
dotnet publish src/TestConsoleApp/TestConsoleApp.csproj -c Release -r win-x64 -p:PublishAot=true
```

### License

MIT. See `LICENSE.txt`.

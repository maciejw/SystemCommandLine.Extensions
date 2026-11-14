## SystemCommandLine.Extensions

[![Build](https://github.com/maciejw/SystemCommandLine.Extensions/actions/workflows/ci-release.yml/badge.svg?branch=master)](https://github.com/maciejw/SystemCommandLine.Extensions/actions/workflows/ci-release.yml)
[![NuGet](https://img.shields.io/nuget/v/SystemCommandLine.Extensions.svg)](https://www.nuget.org/packages/SystemCommandLine.Extensions)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SystemCommandLine.Extensions.svg)](https://www.nuget.org/packages/SystemCommandLine.Extensions)

Small, pragmatic helpers for building command-line apps with
[System.CommandLine](https://www.nuget.org/packages/System.CommandLine) and the
Microsoft.Extensions hosting/DI stack.

- Fluent builders to define options from a typed options class using expressions
- Automatic mapping from parsed command-line values to your options (IOptions<T>)
- DI-first wiring for commands and handlers
- Host-friendly RootCommand setup and ParseResult lifetime integration
- Kebab-case option names by convention (e.g., MyProperty -> --my-property)

### Install

```pwsh
dotnet add package SystemCommandLine.Extensions
```

### Quick start

Define a command and its options using the builder API and expressions. Implement
`IUseCommandBuilder<TCommand>` to expose a factory that the DI extensions can use.
The optional `ConfigureRegistration` method keeps default handler and options
registration with the command.

```csharp
using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Options;
using SystemCommandLine.Extensions;

public class Root : RootCommand, IUseCommandBuilder<Root>
{
    public static Root CommandFactory(IServiceProvider sp, ArgumentMapperRegistration mapperRegistration)
        => new();
}

public class Greet : Command, IUseCommandBuilder<Greet>
{
	public Greet(ArgumentMapperRegistration mapperRegistration)
		: base("greet", "Greets a person")
	{
		// Describe options using your options type and expressions
		this.UseCommandBuilder().WithMapping<GreetOptions>(mapperRegistration)
			.NewOption(o => o.Name).Configure(o =>
			{
				o.Description = "Name to greet";
				o.DefaultValueFactory = _ => "World";
			}).AddToCommand()
			.NewOption(o => o.Times).Configure(o =>
			{
				o.Description = "Number of times";
				o.DefaultValueFactory = _ => 1;
			}).AddToCommand();
	}

	public static Greet CommandFactory(IServiceProvider sp, ArgumentMapperRegistration mapperRegistration)
		=> new(mapperRegistration);

	public static void ConfigureRegistration(CommandRegistrationBuilder<Greet> builder)
	{
		builder.WithAsyncHandler<GreetHandler>().BindOptions<GreetOptions>();
	}
}

public class GreetOptions
{
	public string Name { get; set; } = "World";
	public int Times { get; set; }
}

public class GreetHandler(IOptions<GreetOptions> options) : AsynchronousCommandLineAction
{
    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        for (var i = 0; i < options.Value.Times; i++)
            Console.WriteLine($"Hello, {options.Value.Name}!");
        return Task.FromResult(0);
    }
}
```

Wire it up with hosting and DI. The parsed values are bound into `IOptions<GreetOptions>`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.CommandLine;
using SystemCommandLine.Extensions;

var builder = Host.CreateDefaultBuilder(args)
	.ConfigureServices(services =>
	{
		services.AddRootCommand<Root>(args)
			.AddSubCommand<Greet>();
		services.AddCommand<Greet>();
	});

using IHost host = builder.Start();
ParseResult parse = host.Services.GetRequiredService<ParseResult>();
return await parse.InvokeAsync();
```

See `src/TestConsoleApp` for a complete example, including logging integration.

### Composing commands

A command can attach children inside its `CommandFactory`, using ordinary
`Command.Add` or a collection initializer:

```csharp
return new Root(mapperRegistration)
{
    serviceProvider.GetRequiredService<Greet>()
};
```

To compose commands outside their definitions, use the registration builder
returned by `AddRootCommand<TCommand>` or `AddCommand<TCommand>`:

```csharp
services.AddRootCommand<Root>(args)
    .AddSubCommand<Greet>();
services.AddCommand<Greet>();
```

In this variant, the root factory simply returns `new Root(mapperRegistration)`;
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
| `AddSubCommand<TChild>()` | Attach a separately registered child to this command |
| `Services` | Return to the original service collection |

Every method returns the same command's builder. In particular, `AddSubCommand`
does not switch to the child or register it implicitly. The child supplies its
defaults when separately registered with `AddCommand`. Non-root commands use the same builder, so
the pattern works at every level of a hierarchy. The builder exposes neither a
command instance nor an arbitrary configuration callback; option definitions
remain in the command class.

Register each command once and finish configuring its builder before building the
service provider. Parent and child registrations can appear in either order.
Commands are singletons; children are resolved and attached after the parent's
factory and handler setup, before parsing. Factory-defined children are preserved,
and the same child instance is not attached twice. Repeating `AddSubCommand` for
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

Command factories, fluent option definitions, and the command-level `SetHandler`
and `SetAsyncHandler` methods retain their existing contracts.

`IUseCommandBuilder<TCommand>` now requires
`where TCommand : Command, IUseCommandBuilder<TCommand>` so the hook can accept
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

- .NET 8.0+
- System.CommandLine 2.0.0

To build this repository, use the .NET 11 SDK or newer and install the .NET 8
runtime to run the tests and sample application. The library still targets .NET 8.

```sh
dotnet restore SystemCommandLine.Extensions.slnx
dotnet build SystemCommandLine.Extensions.slnx --configuration Release --no-restore
dotnet test SystemCommandLine.Extensions.slnx --configuration Release --no-build
```

### License

MIT. See `LICENSE.txt`.

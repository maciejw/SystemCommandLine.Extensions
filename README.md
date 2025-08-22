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

```csharp
using System.CommandLine;
using SystemCommandLine.Extensions;

public class Greet : Command, IUseCommandBuilder<Greet>
{
	public Greet(ArgumentMapperRegistration mapperRegistration)
		: base("greet", "Greets a person")
	{
		// Describe options using your options type and expressions
		this.UseCommandBuilder(mapperRegistration).With<GreetOptions>()
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
}

public class GreetOptions
{
	public string Name { get; set; } = "World";
	public int Times { get; set; } = 1;
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
		// Root and command registration
		services.AddRootCommand<Root>(args);
		services.AddCommandWithAsyncHandler<Greet, GreetHandler>();

		// Bind parsed command options into IOptions<T>
		services.AddBoundToCommandOptions<Greet, GreetOptions>();
	});

using IHost host = builder.Start();
ParseResult parse = host.Services.GetRequiredService<ParseResult>();
return await parse.InvokeAsync();
```

See `src/TestConsoleApp` for a complete example, including logging integration.

### Why use this?

System.CommandLine is powerful but low-level. This library smooths out the most
common patterns in real apps:

- Define options once on a POCO and keep them strongly typed end-to-end
- Avoid repetitive glue when mapping parsed values into `IOptions<T>`
- Compose commands and handlers via DI in a host-friendly way

### Requirements

- .NET 8.0+
- System.CommandLine 2.0.0 (preview)

### License

MIT. See `LICENSE.txt`.
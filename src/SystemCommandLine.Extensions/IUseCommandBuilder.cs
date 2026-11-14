#pragma warning disable IDE0130 // Namespace does not match folder structure
using SystemCommandLine.Extensions;

namespace System.CommandLine;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>Defines a command factory and an optional hook for registering the command's defaults.</summary>
/// <typeparam name="TCommand">The command type implementing this contract.</typeparam>
/// <remarks>
/// The factory owns the command definition. The registration hook can select its handler and bind
/// its options, while application startup can compose commands into a hierarchy using the returned registration builder.
/// </remarks>
public interface IUseCommandBuilder<TCommand> where TCommand : Command, IUseCommandBuilder<TCommand>
{
    /// <summary>Creates the command instance and defines its options and any factory-owned children.</summary>
    /// <param name="serviceProvider">The service provider used to resolve the command's dependencies.</param>
    /// <param name="mapperRegistration">The callback used to register mappings from parsed values to options properties.</param>
    /// <returns>The constructed command, before externally registered handler and child configuration is applied.</returns>
    /// <remarks>The DI registration invokes this factory lazily when its singleton command is first resolved.</remarks>
    static abstract TCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration);

    /// <summary>Supplies this command's default handler and options registrations.</summary>
    /// <param name="builder">The registration builder that is subsequently returned to application startup.</param>
    /// <remarks>
    /// Called by AddCommand and AddRootCommand during service registration, before returning the builder.
    /// The default implementation does nothing. Applications can extend the registration or override
    /// the handler through the returned builder. Command instances are still created lazily by CommandFactory.
    /// </remarks>
    static virtual void ConfigureRegistration(CommandRegistrationBuilder<TCommand> builder) { }
}

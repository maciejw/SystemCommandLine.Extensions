using System.ComponentModel;
using SystemCommandLine.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.CommandLine;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>Use ICommandDefinition to define a command factory and registration defaults.</summary>
/// <typeparam name="TCommand">The command type implementing the definition.</typeparam>
[Obsolete("Renamed to ICommandDefinition<TCommand>. Implement ICommandDefinition<TCommand> and rename CommandFactory to CreateCommand.", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IUseCommandBuilder<TCommand>
    where TCommand : Command, IUseCommandBuilder<TCommand>
{
    /// <summary>Use ICommandDefinition.CreateCommand to create the command instance.</summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <param name="mapperRegistration">The callback used to register property mappings.</param>
    /// <returns>The constructed command.</returns>
    [Obsolete("Renamed to CreateCommand on ICommandDefinition<TCommand>. Update your command definition and factory implementation.", error: true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    static abstract TCommand CommandFactory(IServiceProvider serviceProvider, SymbolMapperRegistration mapperRegistration);
}

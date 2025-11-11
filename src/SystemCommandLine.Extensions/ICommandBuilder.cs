using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>
/// Defines a builder interface for constructing command objects with configurable options and argument mappings.
/// </summary>
/// <remarks>Use this interface to fluently add options and configure argument mappings when building command
/// instances. The generic type parameter ensures type safety and enables chaining of builder methods for specific
/// command types.</remarks>
/// <typeparam name="TCommand">The type of command being constructed. Must inherit from Command and implement <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
public interface ICommandBuilder<TCommand> where TCommand : Command, IUseCommandBuilder<TCommand>
{
    /// <summary>
    /// Creates a new option argument for the command with the specified name and value type.
    /// </summary>
    /// <typeparam name="TOption">The type of the option's value. Must be compatible with the command's expected option value type.</typeparam>
    /// <param name="name">The name of the option to add. Cannot be null or empty.</param>
    /// <returns>An argument builder that can be used to further configure the option and add it to the command.</returns>
    ICommandArgumentBuilder<TCommand, TOption> NewOption<TOption>(string name);
    /// <summary>
    /// Associates an argument mapping configuration with the command builder, enabling custom mapping of parsed
    /// arguments to an option holder type.
    /// </summary>
    /// <typeparam name="TOptionHolder">The type that will hold the mapped command-line options. Must be a reference type.</typeparam>
    /// <param name="mapperRegistration">The registration that defines how command-line arguments are mapped to properties of the option holder type.</param>
    /// <returns>An interface that allows further configuration of the command builder with the specified mapping applied.</returns>
    ICommandBuilderWithMapping<TCommand, TOptionHolder> WithMapping<TOptionHolder>(ArgumentMapperRegistration mapperRegistration) where TOptionHolder : class;
}
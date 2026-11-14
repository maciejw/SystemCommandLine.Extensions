using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace SystemCommandLine.Extensions;

/// <summary>Defines command options and positional arguments and starts expression-based mapping to an options type.</summary>
/// <typeparam name="TCommand">The command being defined.</typeparam>
public interface ICommandBuilder<TCommand> where TCommand : Command, ICommandDefinition<TCommand>
{
    /// <summary>Creates a positional argument with a kebab-case name.</summary>
    /// <typeparam name="TValue">The parsed argument value type.</typeparam>
    /// <param name="name">The argument name, without an option prefix.</param>
    /// <returns>An argument builder; call AddToCommand to append the argument.</returns>
    ICommandPositionalArgumentBuilder<TCommand, TValue> NewArgument<TValue>(string name);

    /// <summary>Creates an option builder using the supplied name formatted as a long kebab-case option.</summary>
    /// <typeparam name="TValue">The parsed option value type.</typeparam>
    /// <param name="name">The option name to format, such as <c>LogEventLevel</c> for <c>--log-event-level</c>.</param>
    /// <returns>An option builder; call its <c>AddToCommand</c> method to attach the option.</returns>
    ICommandOptionBuilder<TCommand, TValue> NewOption<TValue>(string name);

    /// <summary>Starts defining options and positional arguments mapped to properties of the specified options type.</summary>
    /// <typeparam name="TOptions">The reference type whose properties receive parsed values.</typeparam>
    /// <param name="mapperRegistration">The mapper-registration delegate supplied to the command factory.</param>
    /// <returns>A builder for defining mapped options and positional arguments on the same command.</returns>
    /// <remarks>Register the options binding with <c>BindOptions&lt;TOptions&gt;()</c> on the command's registration builder.</remarks>
    ICommandBuilderWithMapping<TCommand, TOptions> WithMapping<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptions>(SymbolMapperRegistration mapperRegistration) where TOptions : class;
}

using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>Defines command options and starts expression-based mapping to an options type.</summary>
/// <typeparam name="TCommand">The command being defined.</typeparam>
public interface ICommandBuilder<TCommand> where TCommand : Command, IUseCommandBuilder<TCommand>
{
    /// <summary>Creates an option builder using the supplied name formatted as a long kebab-case option.</summary>
    /// <typeparam name="TOption">The parsed option value type.</typeparam>
    /// <param name="name">The option name to format, such as <c>LogEventLevel</c> for <c>--log-event-level</c>.</param>
    /// <returns>An option builder; call its <c>AddToCommand</c> method to attach the option.</returns>
    ICommandArgumentBuilder<TCommand, TOption> NewOption<TOption>(string name);

    /// <summary>Starts defining options mapped to properties of the specified options type.</summary>
    /// <typeparam name="TOptionHolder">The reference type whose properties receive parsed values.</typeparam>
    /// <param name="mapperRegistration">The mapper-registration delegate supplied to the command factory.</param>
    /// <returns>A builder for defining mapped options on the same command.</returns>
    /// <remarks>Register the options binding with <c>BindOptions&lt;TOptionHolder&gt;()</c> on the command's registration builder.</remarks>
    ICommandBuilderWithMapping<TCommand, TOptionHolder> WithMapping<TOptionHolder>(ArgumentMapperRegistration mapperRegistration) where TOptionHolder : class;
}

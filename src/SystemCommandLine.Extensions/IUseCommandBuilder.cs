#pragma warning disable IDE0130 // Namespace does not match folder structure
using SystemCommandLine.Extensions;

namespace System.CommandLine;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// Defines a contract for building command instances using a factory method that accepts service provider and argument
/// mapping information.
/// </summary>
/// <typeparam name="TCommand">The type of command to be created. Must inherit from <see cref="Command"/>.</typeparam>
public interface IUseCommandBuilder<TCommand> where TCommand : Command
{
    /// <summary>
    /// Creates a new instance of a command using the specified service provider and argument mapper registration.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies required by the command instance.</param>
    /// <param name="mapperRegistration">The registration information for mapping command arguments. In order to use it you need to register using
    /// <see cref="Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddBoundToCommandOptions"/> for the <typeparamref name="TCommand"/>.</param>
    /// <returns>A new instance of the command type <typeparamref name="TCommand"/> configured with the provided services and
    /// argument mapping.</returns>
    static abstract TCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration);
}

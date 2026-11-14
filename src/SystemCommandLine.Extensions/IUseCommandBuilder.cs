#pragma warning disable IDE0130 // Namespace does not match folder structure
using SystemCommandLine.Extensions;

namespace System.CommandLine;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface IUseCommandBuilder<TCommand> where TCommand : Command, IUseCommandBuilder<TCommand>
{
    static abstract TCommand CommandFactory(IServiceProvider serviceProvider, ArgumentMapperRegistration mapperRegistration);

    static virtual void ConfigureRegistration(CommandRegistrationBuilder<TCommand> builder) { }
}

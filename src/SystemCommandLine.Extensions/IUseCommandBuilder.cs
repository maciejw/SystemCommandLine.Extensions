#pragma warning disable IDE0130 // Namespace does not match folder structure
using SystemCommandLine.Extensions;

namespace System.CommandLine;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public interface IUseCommandBuilder<TCommand> where TCommand : Command
{
    static abstract TCommand CommandFactory(IServiceProvider sp, ArgumentMapperRegistration register);
}

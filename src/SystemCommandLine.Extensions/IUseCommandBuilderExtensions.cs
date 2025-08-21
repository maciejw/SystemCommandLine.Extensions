#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.CommandLine;

using SystemCommandLine.Extensions.Builders;

namespace SystemCommandLine.Extensions;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class IUseCommandBuilderExtensions
{
    public static CommandBuilder<TCommand> UseCommandBuilder<TCommand>(this TCommand command, ArgumentMapperRegistration? mapperRegistration = null)
       where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        return new CommandBuilder<TCommand>(command, mapperRegistration);
    }

}

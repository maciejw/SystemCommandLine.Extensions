#pragma warning disable IDE0130 // Namespace does not match folder structure
using SystemCommandLine.Extensions.Builders;

namespace System.CommandLine;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class CommandBuilderExtensions
{
    public static CommandBuilder<TCommand> UsesCommandBuilder<TCommand>(this TCommand command)
       where TCommand : Command, IUseCommandBuilder<TCommand>
    {
        return new CommandBuilder<TCommand>(command);
    }

}

using System.CommandLine;

namespace SystemCommandLine.Extensions;

public interface ICommandBuilder<TCommand> where TCommand : Command, IUseCommandBuilder<TCommand>
{
    ICommandArgumentBuilder<TCommand, TOption> NewOption<TOption>(string name);
    ICommandBuilderWithMapping<TCommand, TOptionHolder> WithMapping<TOptionHolder>(ArgumentMapperRegistration mapperRegistration) where TOptionHolder : class;
}
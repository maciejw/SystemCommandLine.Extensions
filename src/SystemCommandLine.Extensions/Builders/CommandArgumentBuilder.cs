using System.CommandLine;
using System.Linq.Expressions;

namespace SystemCommandLine.Extensions.Builders;

public class CommandArgumentBuilder<TCommand, TOptionHolder>(TCommand command, ArgumentMapperRegistration? mapperRegistration)
    where TCommand : Command, IUseCommandBuilder<TCommand>
    where TOptionHolder : class
{
    public OptionBuilder<TCommand, TOptionHolder, TOption> NewOption<TOption>(Expression<Func<TOptionHolder, TOption>> propertyExpression)
    {
        return new OptionBuilder<TCommand, TOptionHolder, TOption>(command, this, propertyExpression, mapperRegistration);
    }
    public TCommand GetCommand()
    {
        return command;
    }
}
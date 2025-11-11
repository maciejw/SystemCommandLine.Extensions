using System.CommandLine;

namespace SystemCommandLine.Extensions;

/// <summary>
/// Defines a contract for associating a subcommand with a parent command type in a command hierarchy.  
/// </summary>
/// <remarks>Implement this interface to indicate that a command is intended to be used as a subcommand of the
/// specified parent command. This can be used in command line applicationns where commands are in multiple projects and you want subcommand 
/// project to reference parent project, instead of parent command project referencing all its subcommands.</remarks>
/// <typeparam name="TCommand">The type of the parent command that this subcommand is associated with. Must inherit from <see cref="Command"/> and implement
/// <see cref="IUseCommandBuilder{TCommand}"/>.</typeparam>
public interface ISubCommandOf<TCommand> where TCommand : Command, IUseCommandBuilder<TCommand> { }
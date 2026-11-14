using System.CommandLine;
using System.ComponentModel;
using System.Reflection;

namespace SystemCommandLine.Extensions.Tests;

public class ApiMigrationTests
{
    [Theory]
    [InlineData("SystemCommandLine.Extensions.ArgumentMapper", "SymbolMapper")]
    [InlineData("SystemCommandLine.Extensions.ArgumentMapperRegistration", "SymbolMapperRegistration")]
    [InlineData("SystemCommandLine.Extensions.ICommandArgumentBuilder`2", "ICommandOptionBuilder")]
    [InlineData("SystemCommandLine.Extensions.ICommandArgumentBuilderWithMapping`3", "ICommandOptionBuilderWithMapping")]
    [InlineData("System.CommandLine.IUseCommandBuilder`1", "ICommandDefinition")]
    [InlineData("SystemCommandLine.Extensions.IUseCommandBuilderExtensions", "CommandBuilderExtensions")]
    public void Old_types_remain_with_hidden_error_diagnostics_naming_the_replacement(string oldName, string replacement)
    {
        // Resolve old names through metadata because referring to them in source must fail compilation.
        var oldType = typeof(SymbolMapper).Assembly.GetType(oldName, throwOnError: true)!;

        AssertMigrationDiagnostic(oldType, replacement);
    }

    [Theory]
    [InlineData(typeof(ExpressionExtensions), "CreateArgumentValueMapper", "CreatePropertyValueMapper")]
    [InlineData(typeof(CommandRegistrationBuilder<>), "AddSubCommand", "WithSubcommand")]
    public void Renamed_methods_remain_hidden_with_error_diagnostics(Type declaringType, string oldName, string replacement)
    {
        var method = declaringType.GetMethod(oldName);

        Assert.NotNull(method);
        AssertMigrationDiagnostic(method, replacement);
        var newMethod = declaringType.GetMethod(replacement);
        Assert.NotNull(newMethod);
        Assert.Null(newMethod.GetCustomAttribute<ObsoleteAttribute>());
        Assert.NotEqual(EditorBrowsableState.Never, newMethod.GetCustomAttribute<EditorBrowsableAttribute>()?.State);
    }

    [Fact]
    public void Legacy_factory_stays_on_the_old_interface_without_extending_the_new_contract()
    {
        var oldType = typeof(SymbolMapper).Assembly.GetType("System.CommandLine.IUseCommandBuilder`1", throwOnError: true)!;
        var oldFactory = oldType.GetMethod("CommandFactory");

        Assert.NotNull(oldFactory);
        AssertMigrationDiagnostic(oldFactory, "CreateCommand");
        Assert.True(oldFactory.IsAbstract);
        Assert.Empty(oldType.GetInterfaces());
        Assert.Null(typeof(ICommandDefinition<>).GetMethod("CommandFactory"));
        var newFactory = typeof(ICommandDefinition<>).GetMethod("CreateCommand");
        Assert.NotNull(newFactory);
        Assert.True(newFactory.IsAbstract);
        Assert.Null(newFactory.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void Legacy_extension_entry_point_is_hidden_and_does_not_compete_with_the_new_extension()
    {
        var oldType = typeof(SymbolMapper).Assembly.GetType("SystemCommandLine.Extensions.IUseCommandBuilderExtensions", throwOnError: true)!;
        var method = oldType.GetMethod("UseCommandBuilder")!;

        AssertMigrationDiagnostic(method, "CommandBuilderExtensions.UseCommandBuilder");
        Assert.Null(method.GetCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>());
    }

    private static void AssertMigrationDiagnostic(MemberInfo member, string replacement)
    {
        var obsolete = member.GetCustomAttribute<ObsoleteAttribute>();
        Assert.NotNull(obsolete);
        Assert.True(obsolete.IsError);
        Assert.Contains(replacement, obsolete.Message);
        var browsable = member.GetCustomAttribute<EditorBrowsableAttribute>();
        Assert.NotNull(browsable);
        Assert.Equal(EditorBrowsableState.Never, browsable.State);
    }
}

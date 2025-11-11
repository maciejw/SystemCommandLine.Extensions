using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace SystemCommandLine.Extensions;
/// <summary>
/// Provides extension methods for working with strongly-typed property expressions, enabling extraction of property
/// names and creation of value mapping delegates.
/// </summary>
/// <remarks>These methods facilitate reflection-based operations on properties in a type-safe manner, reducing
/// reliance on string-based property references. They are commonly used in scenarios where property metadata or dynamic
/// value assignment is required, such as configuration mapping or model binding.</remarks>
public static class ExpressionExtensions
{
    /// <summary>
    /// Retrieves the name of the property referenced by the specified property expression.
    /// </summary>
    /// <remarks>This method is typically used to obtain property names in a type-safe manner, such as for
    /// data binding, validation, or logging scenarios. The expression must refer to a public property; otherwise, an
    /// exception may be thrown.</remarks>
    /// <typeparam name="TOptionHolder">The type of the object that contains the property.</typeparam>
    /// <typeparam name="TOption">The type of the property being referenced.</typeparam>
    /// <param name="propertyExpression">An expression that specifies the property whose name is to be retrieved. Must reference a public property.</param>
    /// <returns>A string containing the name of the property referenced by the expression.</returns>
    public static string GetPropertyName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptionHolder, TOption>(this Expression<Func<TOptionHolder, TOption>> propertyExpression)
    {
        return propertyExpression.ExtractProperty().Name;
    }
    /// <summary>
    /// Creates an action that sets the value of a specified property on an option holder object using the provided
    /// property expression.
    /// </summary>
    /// <remarks>The returned action will not set the property if the provided value is null. This method is
    /// typically used to map argument values to properties in configuration or options objects.</remarks>
    /// <typeparam name="TOptionHolder">The type of the object that contains the property to be set. Must be a reference type with public properties.</typeparam>
    /// <typeparam name="TOption">The type of the property value to be set.</typeparam>
    /// <param name="propertyExpression">An expression that identifies the property on the option holder to be set. Must refer to a writable property.</param>
    /// <returns>An action that sets the specified property on the option holder to the given value, if the value is not null.</returns>
    /// <exception cref="ArgumentException">Thrown if the property specified by the expression does not have an accessible setter.</exception>
    public static Action<TOptionHolder, TOption?> CreateArgumentValueMapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptionHolder, TOption>(this Expression<Func<TOptionHolder, TOption>> propertyExpression) where TOptionHolder : class
    {
        PropertyInfo propertyInfo = propertyExpression.ExtractProperty();

        if (!propertyInfo.SetMethod.IsWritable())
        {
            throw new ArgumentException($"Property {propertyInfo.DeclaringType!.Name}.{propertyInfo.Name} has no accessible setter.");
        }

        var valueSetter = propertyInfo.SetMethod.CreateDelegate<Action<TOptionHolder, TOption>>();
        return (optionHolder, value) =>
        {
            if (value != null)
            {
                valueSetter(optionHolder, value);
            }
        };
    }

    private static PropertyInfo ExtractProperty<THolder, TProp>(
        this Expression<Func<THolder, TProp>> expr)
    {
        if (expr.Body is MemberExpression memberExpression)
        {
            if (memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo;
            }
            throw new ArgumentException("Expression must target a property");
        }
        throw new ArgumentException("Expression must be a property access like t => t.Property.");
    }

    private static bool IsWritable([NotNullWhen(true)] this MethodInfo? setMethod)
    {
        return setMethod is { } && setMethod.IsPublic;
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace SystemCommandLine.Extensions;

/// <summary>Extracts property metadata and creates setters for expression-based option mapping.</summary>
public static class ExpressionExtensions
{
    /// <summary>Gets the name of the property selected by an expression.</summary>
    /// <typeparam name="TOptionHolder">The type on which the property is selected.</typeparam>
    /// <typeparam name="TOption">The property's value type.</typeparam>
    /// <param name="propertyExpression">A property access such as <c>options =&gt; options.Name</c>.</param>
    /// <returns>The selected property's name, without command-line name formatting.</returns>
    /// <exception cref="ArgumentException">The expression body is not a property member access.</exception>
    public static string GetPropertyName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptionHolder, TOption>(this Expression<Func<TOptionHolder, TOption>> propertyExpression)
    {
        return propertyExpression.ExtractProperty().Name;
    }

    /// <summary>Creates a delegate that assigns non-null values to the selected property.</summary>
    /// <typeparam name="TOptionHolder">The reference type containing the property.</typeparam>
    /// <typeparam name="TOption">The property's value type.</typeparam>
    /// <param name="propertyExpression">A direct property access such as <c>options =&gt; options.Name</c>.</param>
    /// <returns>A delegate accepting the target options instance and the value to assign.</returns>
    /// <remarks>Null values leave the existing property value unchanged. Non-null values, including value-type defaults, are assigned.</remarks>
    /// <exception cref="ArgumentException">The expression is not a property access, the property has no public setter, or its setter is incompatible with the delegate.</exception>
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

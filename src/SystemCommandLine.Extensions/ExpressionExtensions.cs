using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Reflection;

namespace SystemCommandLine.Extensions;

/// <summary>Extracts property metadata and creates setters for mapping option and positional argument values.</summary>
public static class ExpressionExtensions
{
    /// <summary>Gets the name of the property selected by an expression.</summary>
    /// <typeparam name="TOptions">The type on which the property is selected.</typeparam>
    /// <typeparam name="TValue">The property's value type.</typeparam>
    /// <param name="propertyExpression">A property access such as <c>options =&gt; options.Name</c>.</param>
    /// <returns>The selected property's name, without command-line name formatting.</returns>
    /// <exception cref="ArgumentException">The expression body is not a property member access.</exception>
    public static string GetPropertyName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptions, TValue>(this Expression<Func<TOptions, TValue>> propertyExpression)
    {
        return propertyExpression.ExtractProperty().Name;
    }

    /// <summary>Creates a delegate that assigns non-null values to the selected property.</summary>
    /// <typeparam name="TOptions">The reference type containing the property.</typeparam>
    /// <typeparam name="TValue">The property's value type.</typeparam>
    /// <param name="propertyExpression">A direct property access such as <c>options =&gt; options.Name</c>.</param>
    /// <returns>A delegate accepting the target options instance and the value to assign.</returns>
    /// <remarks>Null values leave the existing property value unchanged. Non-null values, including value-type defaults, are assigned.</remarks>
    /// <exception cref="ArgumentException">The expression is not a property access, the property has no public setter, or its setter is incompatible with the delegate.</exception>
    public static Action<TOptions, TValue?> CreatePropertyValueMapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptions, TValue>(this Expression<Func<TOptions, TValue>> propertyExpression) where TOptions : class
    {
        PropertyInfo propertyInfo = propertyExpression.ExtractProperty();

        if (!propertyInfo.SetMethod.IsWritable())
        {
            throw new ArgumentException($"Property {propertyInfo.DeclaringType!.Name}.{propertyInfo.Name} has no accessible setter.");
        }

        var valueSetter = propertyInfo.SetMethod.CreateDelegate<Action<TOptions, TValue>>();
        return (optionHolder, value) =>
        {
            if (value != null)
            {
                valueSetter(optionHolder, value);
            }
        };
    }

    /// <summary>Use CreatePropertyValueMapper to create a property assignment delegate.</summary>
    /// <typeparam name="TOptions">The reference type containing the property.</typeparam>
    /// <typeparam name="TValue">The property's value type.</typeparam>
    /// <param name="propertyExpression">The property to populate.</param>
    /// <returns>A delegate assigning non-null values to the property.</returns>
    [Obsolete("Renamed to CreatePropertyValueMapper. Use CreatePropertyValueMapper instead.", error: true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Action<TOptions, TValue?> CreateArgumentValueMapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptions, TValue>(this Expression<Func<TOptions, TValue>> propertyExpression) where TOptions : class
        => propertyExpression.CreatePropertyValueMapper();

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

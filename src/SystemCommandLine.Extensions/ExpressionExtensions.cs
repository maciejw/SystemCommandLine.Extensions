using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace SystemCommandLine.Extensions;

public static class ExpressionExtensions
{
    public static string GetPropertyName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptionHolder, TOption>(this Expression<Func<TOptionHolder, TOption>> propertyExpression)
    {
        return propertyExpression.ExtractProperty().Name;
    }

    public static Action<TOptionHolder, TOption?> CreateArgumentMapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptionHolder, TOption>(this Expression<Func<TOptionHolder, TOption>> propertyExpression) where TOptionHolder : class
    {
        PropertyInfo propertyInfo = propertyExpression.ExtractProperty();

        if (!propertyInfo.SetMethod.IsWritable())
        {
            throw new ArgumentException($"Property {propertyInfo.DeclaringType!.Name}.{propertyInfo.Name} has no accessible setter.");
        }

        return (Action<TOptionHolder, TOption?>)propertyInfo.SetMethod.CreateDelegate(typeof(Action<TOptionHolder, TOption?>));
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
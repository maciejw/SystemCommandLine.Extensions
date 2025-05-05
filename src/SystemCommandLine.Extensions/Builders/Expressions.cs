using System.Linq.Expressions;
using System.Reflection;

namespace SystemCommandLine.Extensions.Builders;

internal static class Expressions
{
    public static string GetPropertyName<TOptionHolder, TOption>(Expression<Func<TOptionHolder, TOption>> propertyExpression)
    {
        (_, var propertyInfo) = ExtractProperty(propertyExpression);

        return propertyInfo.Name;
    }

    public static Action<TOptionHolder, TOption?> CreateArgumentMapper<TOptionHolder, TOption>(Expression<Func<TOptionHolder, TOption>> propertyExpression) where TOptionHolder : class
    {
        var (memberExpression, propertyInfo) = ExtractProperty(propertyExpression);

        if (!propertyInfo.IsWritable())
        {
            throw new ArgumentException($"Property {propertyInfo.DeclaringType!.Name}.{propertyInfo.Name} has no accessible setter.");
        }

        ParameterExpression holder = propertyExpression.Parameters[0];
        ParameterExpression value = Expression.Parameter(typeof(TOption), "value");
        BinaryExpression assign = Expression.Assign(memberExpression, value);

        return Expression.Lambda<Action<TOptionHolder, TOption?>>(assign, holder, value).Compile();
    }

    private static (MemberExpression memberExpression, PropertyInfo propertyInfo) ExtractProperty<THolder, TProp>(
        Expression<Func<THolder, TProp>> expr)
    {
        if (expr.Body is MemberExpression memberExpression)
        {
            if (memberExpression.Member is PropertyInfo propertyInfo)
            {
                return (memberExpression, propertyInfo);
            }
            throw new ArgumentException("Expression must target a property");
        }
        throw new ArgumentException("Expression must be a property access like t => t.Property.");
    }

    private static bool IsWritable(this PropertyInfo propertyInfo)
    {
        return propertyInfo.SetMethod is { } setMethod && setMethod.IsPublic;
    }
}
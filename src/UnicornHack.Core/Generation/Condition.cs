using System.Linq.Expressions;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Generation;

// TODO: Remove this
public abstract class Condition
{
    protected static readonly ParameterExpression OwnerParameter =
        Expression.Parameter(typeof(GameEntity), name: "owner");

    public Func<Entity, bool> CreateConditionFunction()
    {
        var parameters = new[] { OwnerParameter };

        return Expression.Lambda<Func<Entity, bool>>(GetExpression(), parameters).Compile();
    }

    public abstract Expression GetExpression();
}

public class NoCondition : Condition
{
    public override Expression GetExpression()
        => Expression.Constant(true);
}

public class PropertyCondition : Condition
{
    public string Name
    {
        get;
        set;
    } = null!;

    public ComparisonType Type
    {
        get;
        set;
    }

    public object Value
    {
        get;
        set;
    } = null!;

    public override Expression GetExpression()
    {
        var propertyDescription = PropertyDescription.Loader.Get(Name);
        var componentExpression = Expression.Call(
            OwnerParameter, Entity.FindComponentMethodInfo, Expression.Constant(propertyDescription.ComponentId));
        var getValueMethodInfo = propertyDescription.GetType().GetMethod(nameof(PropertyDescription<int>.GetValue))!;
        var propertyValueExpression = Expression.Call(
            Expression.Constant(propertyDescription), getValueMethodInfo, componentExpression);

        var valueExpression = Expression.Convert(Expression.Constant(Value), propertyDescription.PropertyType);

        switch (Type)
        {
            case ComparisonType.Equal:
                return Expression.Equal(propertyValueExpression, valueExpression);
            case ComparisonType.NotEqual:
                return Expression.NotEqual(propertyValueExpression, valueExpression);
            case ComparisonType.LessThan:
                return Expression.LessThan(propertyValueExpression, valueExpression);
            case ComparisonType.LessThanOrEqual:
                return Expression.LessThanOrEqual(propertyValueExpression, valueExpression);
            case ComparisonType.GreaterThan:
                return Expression.GreaterThan(propertyValueExpression, valueExpression);
            case ComparisonType.GreaterThanOrEqual:
                return Expression.GreaterThanOrEqual(propertyValueExpression, valueExpression);
            default:
                throw new NotImplementedException();
        }
    }
}

public enum ComparisonType
{
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

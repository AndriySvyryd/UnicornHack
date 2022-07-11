using System.Linq.Expressions;

namespace UnicornHack.Generation;

public class UnicornExpressionEvaluatorTest
{
    [Fact]
    public void Literals()
    {
        Assert.Equal(0, Evaluate<int>("0"));
        Assert.Equal("(0)", Evaluate<string>("(('(0)'))"));
    }

    [Fact]
    public void Variables()
    {
        Assert.True(Evaluate<Func<bool, bool>, bool>("$_", ("_", true)));
        Assert.Equal(1, Evaluate<Func<int, int>, int>("$int", ("int", 1)));
        Assert.Equal("true", Evaluate<Func<string, string>, string>("$string", ("string", "true")));
    }

    [Fact]
    public void Strings()
    {
        Assert.Equal("", Evaluate<string>("''"));
        Assert.Equal("foobar", Evaluate<string>("'foo' + 'bar'"));
        Assert.Equal(4, Evaluate<Func<string, int>, int>("$string.Length", ("string", "true")));
    }

    [Fact]
    public void Functions()
    {
        Assert.Equal(-10, Evaluate<int>("Min(-10, 0 )"));
        Assert.Equal(11, Evaluate<int>("Max(2, +11 )"));
        Assert.Equal(100, Evaluate<int>("Abs(-100 )"));
        Assert.Equal(-10, Evaluate<double>("Min(-10, 0.0 )"));
        Assert.Equal(11, Evaluate<double>("Max(2.0, +11 )"));
        Assert.Equal(100, Evaluate<double>("Abs(-100.0 )"));
    }

    [Fact]
    public void Comparisons()
    {
        Assert.False(Evaluate<bool>("-10>0"));
        Assert.True(Evaluate<bool>("-0>=0"));
        Assert.True(Evaluate<bool>(" 2< +11"));
        Assert.False(Evaluate<bool>(" 22<= +11"));
        Assert.False(Evaluate<bool>("-100 == 100"));
        Assert.True(Evaluate<bool>("1 != 100"));
    }

    [Fact]
    public void Bool_Operations()
    {
        Assert.False(Evaluate<bool>("-10>0 && -0>=0"));
        Assert.True(Evaluate<bool>(" 2< +11 || 22<= +11"));
        Assert.True(Evaluate<bool>("(-100 == 100) == (10 != 10)"));
        Assert.False(Evaluate<bool>("!(1 != 100)"));
    }

    [Fact]
    public void Number_Operations()
    {
        Assert.Equal(0, Evaluate<double>("2.0 + 2.0 ^ 2 - 3 * 10.0 / 5.0"));
        Assert.Equal(1.5, Evaluate<double>("3.0 / 2.0"));
        Assert.Equal(double.PositiveInfinity, Evaluate<double>("2 * Infinity"));
        Assert.Equal(double.PositiveInfinity, Evaluate<double>("1.0 / 0.0"));
        Assert.Equal(double.NegativeInfinity, Evaluate<double>("-1.0 / 0.0"));
    }

    private TResult Evaluate<TResult>(string expression)
        => Evaluate<Func<TResult>, TResult>(expression);

    private TResult Evaluate<T, TResult>(string expression, params (string Name, object Value)[] parameters)
        where T : Delegate
    {
        var parameterExpressions = new List<ParameterExpression>();
        foreach (var parameter in parameters)
        {
            parameterExpressions.Add(Expression.Parameter(parameter.Value.GetType(), parameter.Name));
        }

        var translator = new UnicornExpressionVisitor(parameterExpressions);

        var compiledLambda = (Delegate)translator.Translate<T, TResult>(expression);

        return (TResult)compiledLambda.DynamicInvoke(parameters.Select(v => v.Value).ToArray())!;
    }
}

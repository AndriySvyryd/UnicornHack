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
    public void Log_natural()
    {
        Assert.Equal(0.0, Evaluate<double>("Log(1)"));
        Assert.Equal(0.0, Evaluate<double>("Log(1.0)"));
        Assert.Equal(Math.Log(2.0), Evaluate<double>("Log(2)"));
        Assert.Equal(Math.Log(10.0), Evaluate<double>("Log(10.0)"));
        Assert.Equal(double.NegativeInfinity, Evaluate<double>("Log(0)"));
        Assert.True(double.IsNaN(Evaluate<double>("Log(-1.0)")));
    }

    [Fact]
    public void Log_with_base()
    {
        Assert.Equal(3.0, Evaluate<double>("Log(8.0, 2.0)"));
        Assert.Equal(2.0, Evaluate<double>("Log(100, 10)"));
        Assert.Equal(0.0, Evaluate<double>("Log(1, 10)"));
        // Same base as value yields 1.
        Assert.Equal(1.0, Evaluate<double>("Log(5, 5)"));
    }

    [Fact]
    public void Log_invalid_arity_throws()
    {
        Assert.Throws<InvalidOperationException>(() => Evaluate<double>("Log()"));
        Assert.Throws<InvalidOperationException>(() => Evaluate<double>("Log(1, 2, 3)"));
    }

    [Fact]
    public void Shift_operators()
    {
        // Left shift
        Assert.Equal(8, Evaluate<int>("1 << 3"));
        Assert.Equal(40, Evaluate<int>("5 << 3"));
        Assert.Equal(0, Evaluate<int>("0 << 5"));

        // Right shift
        Assert.Equal(1, Evaluate<int>("8 >> 3"));
        Assert.Equal(5, Evaluate<int>("40 >> 3"));
        Assert.Equal(0, Evaluate<int>("3 >> 5"));

        // Arithmetic (sign-preserving) right shift for negatives
        Assert.Equal(-1, Evaluate<int>("-1 >> 5"));
        Assert.Equal(-5, Evaluate<int>("-40 >> 3"));
    }

    [Fact]
    public void Shift_precedence()
    {
        // Shifts have lower precedence than additive: 1 + 1 << 2 == (1+1) << 2 == 8
        Assert.Equal(8, Evaluate<int>("1 + 1 << 2"));
        // And higher precedence than relational: 1 << 2 == 4 -> equality holds
        Assert.True(Evaluate<bool>("1 << 2 == 4"));
        Assert.True(Evaluate<bool>("8 >> 1 == 4"));
        // Same-precedence shifts associate left-to-right: 16 >> 1 >> 1 == (16>>1)>>1 == 4
        Assert.Equal(4, Evaluate<int>("16 >> 1 >> 1"));
        // Combined with multiplicative: 4 * 2 << 1 == (4*2)<<1 == 16 (mul binds tighter than shift)
        Assert.Equal(16, Evaluate<int>("4 * 2 << 1"));
    }

    [Fact]
    public void Shift_truncates_double_operand()
    {
        // A double operand is truncated toward zero per int conversion semantics.
        Assert.Equal(4, Evaluate<int>("2.7 << 1"));
        Assert.Equal(2, Evaluate<int>("9.9 >> 2"));
    }

    [Fact]
    public void Shift_in_weight_formula()
    {
        // The fragment-weight pattern: w / ((1 + $instances) >> 1) using floating-point
        // numerator. Verifies the formula yields the expected shape including the
        // +Infinity sentinel at $instances == 0 (which forces first-pick).
        Assert.Equal(double.PositiveInfinity,
            Evaluate<Func<int, double>, double>("30.0 / ((1 + $instances) >> 1)", ("instances", 0)));
        Assert.Equal(30.0,
            Evaluate<Func<int, double>, double>("30.0 / ((1 + $instances) >> 1)", ("instances", 1)));
        Assert.Equal(30.0,
            Evaluate<Func<int, double>, double>("30.0 / ((1 + $instances) >> 1)", ("instances", 2)));
        Assert.Equal(15.0,
            Evaluate<Func<int, double>, double>("30.0 / ((1 + $instances) >> 1)", ("instances", 3)));
        Assert.Equal(10.0,
            Evaluate<Func<int, double>, double>("30.0 / ((1 + $instances) >> 1)", ("instances", 5)));
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

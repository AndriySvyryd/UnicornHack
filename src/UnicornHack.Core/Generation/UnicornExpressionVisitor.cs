using System.Linq.Expressions;
using System.Reflection;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using static System.Linq.Expressions.Expression;

namespace UnicornHack.Generation;

public class UnicornExpressionVisitor : UnicornExpressionBaseVisitor<Expression>
{
    private readonly IEnumerable<ParameterExpression> _orderedParameters;

    private static readonly MethodInfo _minMethod =
        typeof(Math).GetMethod(nameof(Math.Min), [typeof(int), typeof(int)])!;

    private static readonly MethodInfo _minMethodD =
        typeof(Math).GetMethod(nameof(Math.Min), [typeof(double), typeof(double)])!;

    private static readonly MethodInfo _maxMethod =
        typeof(Math).GetMethod(nameof(Math.Max), [typeof(int), typeof(int)])!;

    private static readonly MethodInfo _maxMethodD =
        typeof(Math).GetMethod(nameof(Math.Max), [typeof(double), typeof(double)])!;

    private static readonly MethodInfo _absMethod = typeof(Math).GetMethod(nameof(Math.Abs), [typeof(int)])!;

    private static readonly MethodInfo _absMethodD =
        typeof(Math).GetMethod(nameof(Math.Abs), [typeof(double)])!;

    private static readonly MethodInfo _logMethod =
        typeof(Math).GetMethod(nameof(Math.Log), [typeof(double)])!;

    private static readonly MethodInfo _logMethodBase =
        typeof(Math).GetMethod(nameof(Math.Log), [typeof(double), typeof(double)])!;

    private static readonly MethodInfo _stringConcat = typeof(string)
        .GetRequiredRuntimeMethod(nameof(string.Concat), typeof(string), typeof(string));

    public UnicornExpressionVisitor(IEnumerable<ParameterExpression> parameters)
    {
        _orderedParameters = parameters;
        foreach (var parameter in parameters)
        {
            Parameters[parameter.Name!] = parameter;
        }
    }

    /// <summary>
    ///     Variant that lets a caller bind named DSL variables to arbitrary precomputed
    ///     expressions instead of straight lambda parameters. The <paramref name="parameters" />
    ///     list still drives the compiled lambda's signature; <paramref name="variableOverrides" />
    ///     replaces the parser-time binding for the named variables, so an author who
    ///     writes <c>$myVar</c> in the expression gets the override expression baked into
    ///     the compiled IL at that AST node.
    /// </summary>
    public UnicornExpressionVisitor(
        IEnumerable<ParameterExpression> parameters,
        IReadOnlyDictionary<string, Expression> variableOverrides)
        : this(parameters)
    {
        foreach (var (name, expression) in variableOverrides)
        {
            Parameters[name] = expression;
        }
    }

    protected Dictionary<string, Expression> Parameters
    {
        get;
    } = new();

    public T Translate<T, TResult>(string expression)
    {
        var inputStream = new AntlrInputStream(expression);
        var unicornLexer = new UnicornExpressionLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(unicornLexer);
        var outputWriter = new StringWriter();
        var errorWriter = new StringWriter();
        var unicornParser = new UnicornExpressionParser(commonTokenStream, outputWriter, errorWriter);

        var context = unicornParser.expression();

        Expression result;
        try
        {
            result = Visit(context);
        }
        catch (UnicornExpressionException)
        {
            var error = errorWriter.ToString();
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(error);
            }

            throw;
        }

        var output = outputWriter.ToString();
        if (!string.IsNullOrEmpty(output))
        {
            throw new InvalidOperationException(output);
        }

        if (result.Type != typeof(TResult))
        {
            result = Convert(result, typeof(TResult));
        }

        return Lambda<T>(result, _orderedParameters).Compile();
    }

    public override Expression VisitErrorNode(IErrorNode node)
        => throw new InvalidOperationException(node.ToString());

    public override Expression VisitChildren(IRuleNode node) => throw new UnicornExpressionException();

    protected override Expression AggregateResult(Expression aggregate, Expression nextResult)
        => throw new InvalidOperationException();

    protected override bool ShouldVisitNextChild(IRuleNode node, Expression currentResult)
        => throw new InvalidOperationException();

    public override Expression VisitConditionalExpression(
        UnicornExpressionParser.ConditionalExpressionContext context)
    {
        var ifTrue = Visit(context.True);
        var ifFalse = Visit(context.False);
        var commonType = GetCommonType(ifTrue, ifFalse);
        return Condition(Visit(context.Condition), Convert(ifTrue, commonType), Convert(ifFalse, commonType));
    }

    public override Expression VisitConditionalOrExpression(
        UnicornExpressionParser.ConditionalOrExpressionContext context)
        => OrElse(Visit(context.Left), Visit(context.Right));

    public override Expression VisitConditionalAndExpression(
        UnicornExpressionParser.ConditionalAndExpressionContext context)
        => AndAlso(Visit(context.Left), Visit(context.Right));

    public override Expression VisitPowerExpression(
        UnicornExpressionParser.PowerExpressionContext context)
    {
        var left = Visit(context.Left);
        var right = Visit(context.Right);
        var commonType = GetCommonType(left, right);
        return Power(Convert(left, commonType), Convert(right, commonType));
    }

    public override Expression VisitRelationalExpression(
        UnicornExpressionParser.RelationalExpressionContext context)
    {
        var left = Visit(context.Left);
        var right = Visit(context.Right);
        var commonType = GetCommonType(left, right);
        left = Convert(left, commonType);
        right = Convert(right, commonType);
        if (context.LT() != null)
        {
            return LessThan(left, right);
        }

        if (context.GT() != null)
        {
            return GreaterThan(left, right);
        }

        if (context.LE() != null)
        {
            return LessThanOrEqual(left, right);
        }

        if (context.GE() != null)
        {
            return GreaterThanOrEqual(left, right);
        }

        if (context.NEQ() != null)
        {
            return NotEqual(left, right);
        }

        return Equal(left, right);
    }

    public override Expression VisitAdditiveExpression(
        UnicornExpressionParser.AdditiveExpressionContext context)
    {
        var left = Visit(context.Left);
        var right = Visit(context.Right);
        var commonType = GetCommonType(left, right);
        left = Convert(left, commonType);
        right = Convert(right, commonType);

        return context.MINUS() != null
            ? Subtract(left, right)
            : commonType == typeof(string)
                ? Add(left, right, _stringConcat)
                : Add(left, right);
    }

    public override Expression VisitMultiplicativeExpression(
        UnicornExpressionParser.MultiplicativeExpressionContext context)
    {
        var left = Visit(context.Left);
        var right = Visit(context.Right);
        var commonType = GetCommonType(left, right);
        left = Convert(left, commonType);
        right = Convert(right, commonType);

        return context.ASTERISK() != null ? Multiply(left, right) : Divide(left, right);
    }

    public override Expression VisitShiftExpression(
        UnicornExpressionParser.ShiftExpressionContext context)
    {
        // Shift operators (<<, >>) are bitwise and require integral operands. Coerce
        // both sides to int; the shift count must also be int per Expression.LeftShift /
        // RightShift. Truncates any double operand toward zero, matching C# casting.
        var left = Visit(context.Left);
        var right = Visit(context.Right);
        if (left.Type != typeof(int))
        {
            left = Convert(left, typeof(int));
        }

        if (right.Type != typeof(int))
        {
            right = Convert(right, typeof(int));
        }

        return context.LSHIFT() != null ? LeftShift(left, right) : RightShift(left, right);
    }

    public override Expression VisitUnaryExpression(
        UnicornExpressionParser.UnaryExpressionContext context)
    {
        if (context.exception != null)
        {
            throw new UnicornExpressionException(context.exception);
        }

        var expression = Visit(context.Expression);

        if (context.EXCLAMATION() != null)
        {
            return Not(expression);
        }

        if (context.MINUS() != null)
        {
            return Negate(expression);
        }

        return expression;
    }

    public override Expression VisitIntegerLiteralExpression(
        UnicornExpressionParser.IntegerLiteralExpressionContext context)
        => Constant(int.Parse(context.Integer.Text));

    public override Expression VisitFloatLiteralExpression(
        UnicornExpressionParser.FloatLiteralExpressionContext context)
        => Constant(double.Parse(context.Float.Text));

    public override Expression VisitStringLiteralExpression(
        UnicornExpressionParser.StringLiteralExpressionContext context)
        => Constant(context.String.Text[1..^1]);

    public override Expression VisitConstantExpression(
        UnicornExpressionParser.ConstantExpressionContext context)
        => Constant(double.PositiveInfinity);

    public override Expression VisitAccessExpression(
        UnicornExpressionParser.AccessExpressionContext context)
    {
        if (context.exception != null)
        {
            throw new UnicornExpressionException(context.exception);
        }

        var expression = Visit(context.Expression);
        var member = expression.Type.GetMember(context.Member.Text).Single();
        return MakeMemberAccess(expression, member);
    }

    public override Expression VisitVariableExpression(
        UnicornExpressionParser.VariableExpressionContext context)
    {
        if (context.exception != null)
        {
            throw new UnicornExpressionException(context.exception);
        }

        return Parameters[context.Variable.Text];
    }

    public override Expression VisitIndexedVariableExpression(
        UnicornExpressionParser.IndexedVariableExpressionContext context)
    {
        if (context.exception != null)
        {
            throw new UnicornExpressionException(context.exception);
        }

        var parameter = Parameters[context.Variable.Text];
        var key = Visit(context.Index);

        // Resolve a GetValueOrDefault overload from the parameter's runtime dictionary type.
        // We only support IReadOnlyDictionary<,> today; that's the only declared indexed
        // parameter ($tagInstances). Throw eagerly for any other parameter so the author
        // gets a useful error at script-compile time.
        var dictInterface = (parameter.Type.IsGenericType
                && parameter.Type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                ? parameter.Type
                : null)
            ?? parameter.Type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
            ?? throw new InvalidOperationException(
                $"Indexer access '[]' is only supported on IReadOnlyDictionary<,> parameters; '${context.Variable.Text}' has type {parameter.Type}.");

        var typeArgs = dictInterface.GetGenericArguments();
        var keyType = typeArgs[0];
        var valueType = typeArgs[1];
        var getOrDefault = typeof(CollectionExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(CollectionExtensions.GetValueOrDefault)
                && m.GetParameters().Length == 2)
            .MakeGenericMethod(keyType, valueType);

        var convertedKey = key.Type == keyType ? key : Convert(key, keyType);
        return Call(null, getOrDefault, Convert(parameter, dictInterface), convertedKey);
    }

    public override Expression VisitInvocationExpression(
        UnicornExpressionParser.InvocationExpressionContext context)
    {
        if (context.exception != null)
        {
            throw new UnicornExpressionException(context.exception);
        }

        var children = context.expression().Select(Visit).ToList();

        var commonType = children.Count == 0
            ? typeof(object)
            : children.Count > 1
                ? GetCommonType(children[0], children[1])
                : children[0].Type;

        var function = context.Function.Text;

        return CreateInvocation(function, children, commonType);
    }

    protected virtual Expression CreateInvocation(
        string function, IEnumerable<Expression> children, Type childrenCommonType)
    {
        MethodInfo method;
        switch (function)
        {
            case "Max":
                method = childrenCommonType == typeof(double) ? _maxMethodD : _maxMethod;
                break;
            case "Min":
                method = childrenCommonType == typeof(double) ? _minMethodD : _minMethod;
                break;
            case "Abs":
                method = childrenCommonType == typeof(double) ? _absMethodD : _absMethod;
                break;
            case "Log":
            {
                // Log accepts either one argument (natural log) or two arguments
                // (value, base). Both arguments and the result are double — we route
                // through Math.Log so callers can write `Log($instances + 1)` or
                // `Log($instances + 1, 2)` for a base-2 log.
                var args = children.Select(c => Convert(c, typeof(double))).ToArray();
                return args.Length switch
                {
                    1 => Call(null, _logMethod, args[0]),
                    2 => Call(null, _logMethodBase, args[0], args[1]),
                    _ => throw new InvalidOperationException(
                        $"Function Log expects 1 or 2 arguments but got {args.Length}")
                };
            }
            default:
                throw new InvalidOperationException($"Function {function} not supported");
        }

        var convertedChildren = children.Select(c => Convert(c, childrenCommonType));
        return Call(null, method, convertedChildren);
    }

    public override Expression VisitParenthesizedExpression(
        UnicornExpressionParser.ParenthesizedExpressionContext context)
        => Visit(context.expression());

    private static Type GetCommonType(Expression left, Expression right)
        => left.Type == right.Type
            ? left.Type
            : left.Type.IsNumeric() && right.Type.IsNumeric()
                ? typeof(double)
                : typeof(object);

    private Expression Convert(Expression left, Type type)
        => left.Type != type ? Expression.Convert(left, type) : left;
}

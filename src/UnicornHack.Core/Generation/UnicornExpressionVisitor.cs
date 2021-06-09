using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using UnicornHack.Utils;
using static System.Linq.Expressions.Expression;

namespace UnicornHack.Generation
{
    public class UnicornExpressionVisitor : UnicornExpressionBaseVisitor<Expression>
    {
        private IEnumerable<ParameterExpression> _orderedParameters;
        private static readonly MethodInfo _minMethod = typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) });
        private static readonly MethodInfo _minMethodD = typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) });
        private static readonly MethodInfo _maxMethod = typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) });
        private static readonly MethodInfo _maxMethodD = typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) });
        private static readonly MethodInfo _absMethod = typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(int) });
        private static readonly MethodInfo _absMethodD = typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) });
        private static readonly MethodInfo _stringConcat = typeof(string)
            .GetRequiredRuntimeMethod(nameof(string.Concat), typeof(string), typeof(string));

        public UnicornExpressionVisitor(IEnumerable<ParameterExpression> parameters)
        {
            _orderedParameters = parameters;
            foreach (var parameter in parameters)
            {
                Parameters[parameter.Name] = parameter;
            }
        }

        protected Dictionary<string, ParameterExpression> Parameters { get; } = new Dictionary<string, ParameterExpression>();

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
        {
            throw new InvalidOperationException(node.ToString());
        }

        public override Expression VisitChildren(IRuleNode node)
        {
            throw new UnicornExpressionException();
        }

        protected override Expression AggregateResult(Expression aggregate, Expression nextResult)
        {
            throw new InvalidOperationException();
        }

        protected override bool ShouldVisitNextChild(IRuleNode node, Expression currentResult)
        {
            throw new InvalidOperationException();
        }

        public override Expression VisitConditionalExpression([NotNull] UnicornExpressionParser.ConditionalExpressionContext context)
        {
            var ifTrue = Visit(context.True);
            var ifFalse = Visit(context.False);
            var commonType = GetCommonType(ifTrue, ifFalse);
            return Condition(Visit(context.Condition), Convert(ifTrue, commonType), Convert(ifFalse, commonType));
        }

        public override Expression VisitConditionalOrExpression([NotNull] UnicornExpressionParser.ConditionalOrExpressionContext context)
            => OrElse(Visit(context.Left), Visit(context.Right));

        public override Expression VisitConditionalAndExpression([NotNull] UnicornExpressionParser.ConditionalAndExpressionContext context)
            => AndAlso(Visit(context.Left), Visit(context.Right));

        public override Expression VisitPowerExpression([NotNull] UnicornExpressionParser.PowerExpressionContext context)
        {
            var left = Visit(context.Left);
            var right = Visit(context.Right);
            var commonType = GetCommonType(left, right);
            return Power(Convert(left, commonType), Convert(right, commonType));
        }

        public override Expression VisitRelationalExpression([NotNull] UnicornExpressionParser.RelationalExpressionContext context)
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

        public override Expression VisitAdditiveExpression([NotNull] UnicornExpressionParser.AdditiveExpressionContext context)
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

        public override Expression VisitMultiplicativeExpression([NotNull] UnicornExpressionParser.MultiplicativeExpressionContext context)
        {
            var left = Visit(context.Left);
            var right = Visit(context.Right);
            var commonType = GetCommonType(left, right);
            left = Convert(left, commonType);
            right = Convert(right, commonType);

            return context.ASTERISK() != null ? Multiply(left, right) : Divide(left, right);
        }

        public override Expression VisitUnaryExpression([NotNull] UnicornExpressionParser.UnaryExpressionContext context)
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

        public override Expression VisitIntegerLiteralExpression([NotNull] UnicornExpressionParser.IntegerLiteralExpressionContext context)
            => Constant(int.Parse(context.Integer.Text));

        public override Expression VisitFloatLiteralExpression([NotNull] UnicornExpressionParser.FloatLiteralExpressionContext context)
            => Constant(double.Parse(context.Float.Text));

        public override Expression VisitStringLiteralExpression([NotNull] UnicornExpressionParser.StringLiteralExpressionContext context)
            => Constant(context.String.Text[1..^1]);

        public override Expression VisitConstantExpression([NotNull] UnicornExpressionParser.ConstantExpressionContext context)
            => Constant(double.PositiveInfinity);

        public override Expression VisitAccessExpression([NotNull] UnicornExpressionParser.AccessExpressionContext context)
        {
            if (context.exception != null)
            {
                throw new UnicornExpressionException(context.exception);
            }

            var expression = Visit(context.Expression);
            var member = expression.Type.GetMember(context.Member.Text).Single();
            return MakeMemberAccess(expression, member);
        }

        public override Expression VisitVariableExpression([NotNull] UnicornExpressionParser.VariableExpressionContext context)
        {
            if (context.exception != null)
            {
                throw new UnicornExpressionException(context.exception);
            }

            return Parameters[context.Variable.Text];
        }

        public override Expression VisitInvocationExpression([NotNull] UnicornExpressionParser.InvocationExpressionContext context)
        {
            if (context.exception != null)
            {
                throw new UnicornExpressionException(context.exception);
            }

            var children = context.expression().Select(c => Visit(c)).ToList();

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
                default:
                    throw new InvalidOperationException($"Function {function} not supported");
            }

            var convertedChildren = children.Select(c => Convert(c, childrenCommonType));
            return Call(null, method, convertedChildren);
        }

        public override Expression VisitParenthesizedExpression([NotNull] UnicornExpressionParser.ParenthesizedExpressionContext context)
            => Visit(context.expression());

        private Type GetCommonType(Expression left, Expression right)
            => left.Type == right.Type
                ? left.Type
                : left.Type.IsNumeric() && right.Type.IsNumeric()
                    ? typeof(double)
                    : typeof(object);

        private Expression Convert(Expression left, Type type) => left.Type != type ? Expression.Convert(left, type) : left;
    }
}

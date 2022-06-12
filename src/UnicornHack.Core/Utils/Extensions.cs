using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Utils
{
    public static class Extensions
    {
        public static IReadOnlyCollection<T> GetFlags<T>(this T flags)
        {
            var values = new List<T>();
            var defaultValue = Enum.ToObject(typeof(T), value: 0);
            foreach (Enum enumValue in Enum.GetValues(typeof(T)))
            {
                if (enumValue.Equals(defaultValue))
                {
                    continue;
                }

                if (((Enum)(object)flags).HasFlag(enumValue))
                {
                    values.Add((T)(object)enumValue);
                }
            }

            return values;
        }

        public static IReadOnlyCollection<T> GetNonRedundantFlags<T>(this T flags, bool removeComposites)
        {
            var values = new HashSet<T>(flags.GetFlags());
            foreach (var currentValue in values.ToList())
            {
                var decomposedValues = currentValue.GetFlags();
                if (decomposedValues.Count > 1)
                {
                    if (removeComposites)
                    {
                        values.Remove(currentValue);
                    }
                    else
                    {
                        values.ExceptWith(decomposedValues.Where(v => !EqualityComparer<T>.Default.Equals(v, currentValue)));
                    }
                }
            }

            return values;
        }

        public static ICollection<T> With<T>(this ICollection<T> list, T item)
        {
            list.Add(item);
            return list;
        }

        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }

            return collection;
        }

        public static Dictionary<TKey, TValue> AddRange<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary, IEnumerable<TKey> items, Func<TKey, TValue> selector)
        {
            foreach (var item in items)
            {
                dictionary.Add(item, selector(item));
            }

            return dictionary;
        }

        public static Queue<T> EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }

            return queue;
        }

        public static IEnumerable<Point> AsPoints(this IEnumerable<byte> bytes)
        {
            using (var enumerable = bytes.GetEnumerator())
            {
                while (enumerable.MoveNext())
                {
                    var x = enumerable.Current;
                    enumerable.MoveNext();
                    var y = enumerable.Current;
                    yield return new Point(x, y);
                }
            }
        }

        public static IEnumerable<byte> AsBytes(this IEnumerable<Point> points)
        {
            foreach (var point in points)
            {
                yield return point.X;
                yield return point.Y;
            }
        }

        public static PropertyInfo GetSimplePropertyAccess([NotNull] this LambdaExpression propertyAccessExpression)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var parameterExpression = propertyAccessExpression.Parameters.Single();
            var propertyInfo = parameterExpression.MatchSimplePropertyAccess(propertyAccessExpression.Body);

            if (propertyInfo == null)
            {
                throw new ArgumentException(nameof(propertyAccessExpression));
            }

            return BindRuntimeProperty(propertyInfo, parameterExpression.Type);
        }

        private static PropertyInfo MatchSimplePropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            var propertyInfos = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return propertyInfos?.Count == 1 ? propertyInfos[0] : null;
        }

        private static IReadOnlyList<PropertyInfo> MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = RemoveTypeAs(RemoveConvert(propertyAccessExpression)) as MemberExpression;

                var propertyInfo = memberExpression?.Member as PropertyInfo;

                if (propertyInfo == null)
                {
                    return null;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            } while (RemoveTypeAs(RemoveConvert(memberExpression.Expression)) != parameterExpression);

            return propertyInfos;
        }

        public static Expression RemoveConvert([CanBeNull] this Expression expression)
        {
            while (expression != null
                   && (expression.NodeType == ExpressionType.Convert
                       || expression.NodeType == ExpressionType.ConvertChecked))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }

        public static Expression RemoveTypeAs([CanBeNull] this Expression expression)
        {
            while (expression?.NodeType == ExpressionType.TypeAs)
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }

        private static PropertyInfo BindRuntimeProperty(PropertyInfo propertyInfo, Type parameterType)
        {
            var declaringType = propertyInfo?.DeclaringType;

            if (declaringType == null
                || declaringType == parameterType
                || !declaringType.GetTypeInfo().IsInterface
                || !declaringType.GetTypeInfo().IsAssignableFrom(parameterType.GetTypeInfo()))
            {
                return propertyInfo;
            }

            var propertyGetter = propertyInfo.GetMethod;
            if (propertyGetter == null)
            {
                return null;
            }

            var interfaceMapping = parameterType.GetTypeInfo().GetRuntimeInterfaceMap(declaringType);
            var index = Array.FindIndex(interfaceMapping.InterfaceMethods, p => propertyGetter.Equals(p));
            var targetMethod = interfaceMapping.TargetMethods[index];
            foreach (var runtimeProperty in parameterType.GetRuntimeProperties())
            {
                if (targetMethod.Equals(runtimeProperty.GetMethod))
                {
                    return runtimeProperty;
                }
            }

            return propertyInfo;
        }

        public static (Delegate Get, Delegate ComponentGet, Delegate Set) GetPropertyAccessors(
            [NotNull] this LambdaExpression propertyAccessExpression)
            => new PropertyAccessorExpressionVisitor().GetPropertyAccessors(propertyAccessExpression);

        private class PropertyAccessorExpressionVisitor : ExpressionVisitor
        {
            private ParameterExpression _rootParameter;
            private Type _resultType;
            private ParameterExpression _valueParameter;
            private ParameterExpression _componentParameter;
            private Expression _componentGetExpression;
            private Expression _setExpression;
            private bool _isLastMember;

            public (Delegate Get, Delegate ComponentGet, Delegate Set) GetPropertyAccessors(
                [NotNull] LambdaExpression propertyAccessExpression)
            {
                if (propertyAccessExpression.Parameters.Count != 1)
                {
                    throw new InvalidOperationException("Expected a lambda with only 1 parameter, got "
                                                        + propertyAccessExpression.Parameters.Count);
                }

                _rootParameter = propertyAccessExpression.Parameters.Single();
                _resultType = propertyAccessExpression.Body.Type;
                _valueParameter = Expression.Parameter(_resultType);
                _componentParameter = Expression.Parameter(typeof(Component));

                _isLastMember = true;

                var getExpression = Visit(propertyAccessExpression.Body);

                var get = Expression.Lambda(getExpression, _rootParameter).Compile();
                var componentGet = _componentGetExpression == null
                    ? null
                    : Expression.Lambda(_componentGetExpression, _componentParameter).Compile();
                var set = _setExpression == null
                    ? null
                    : Expression.Lambda(_setExpression, _rootParameter, _valueParameter).Compile();
                return (get, componentGet, set);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                _componentGetExpression = null;
                _setExpression = _rootParameter;
                return _rootParameter;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType is ExpressionType.Convert
                    or ExpressionType.ConvertChecked
                    or ExpressionType.TypeAs)
                {
                    return base.Visit(node.Operand);
                }

                throw new InvalidOperationException("Unexpected expression type " + node.NodeType);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.NodeType == ExpressionType.Coalesce)
                {
                    var left = base.Visit(node.Left);
                    var leftSet = _setExpression;
                    var leftComponentGetExpression = _componentGetExpression;

                    _isLastMember = true;
                    var right = base.Visit(node.Right);

                    if (_setExpression == null)
                    {
                        _setExpression = leftSet;
                    }
                    else if (leftSet == null)
                    {
                    }
                    else if (leftSet is not BlockExpression leftBlock)
                    {
                        _setExpression = Expression.Block(new[] {leftSet, _setExpression});
                    }
                    else if (_setExpression is BlockExpression rightBlock)
                    {
                        _setExpression = leftBlock.Update(leftBlock.Variables.Concat(rightBlock.Variables),
                            leftBlock.Expressions.Concat(rightBlock.Expressions));
                    }
                    else
                    {
                        _setExpression = leftBlock.Update(leftBlock.Variables,
                            leftBlock.Expressions.Concat(new[] {_setExpression}));
                    }

                    _componentGetExpression = Expression.Coalesce(leftComponentGetExpression, _componentGetExpression);
                    return Expression.Coalesce(left, right);
                }

                throw new InvalidOperationException("Unexpected expression type " + node.NodeType);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var propertyInfo = node.Member as PropertyInfo;
                if (propertyInfo == null)
                {
                    throw new InvalidOperationException(node.Member.Name + " is not a property, only properties are supported.");
                }

                var isLastMember = _isLastMember;
                _isLastMember = false;
                var getExpression = base.Visit(node.Expression);

                var runtimeProperty = BindRuntimeProperty(propertyInfo, getExpression.Type);
                var type = runtimeProperty.PropertyType;

                if (runtimeProperty == null)
                {
                    throw new InvalidOperationException($"Property {propertyInfo.Name} is not present on {node.Expression?.Type.Name}");
                }

                if (type.IsAssignableTo(typeof(Component)))
                {
                    _componentGetExpression = Expression.TypeAs(_componentParameter, type);
                }
                else if (_componentGetExpression != null)
                {
                    var componentCondition = Expression.Equal(_componentGetExpression,
                        Expression.Constant(null, _componentGetExpression.Type));
                    _componentGetExpression = Expression.Property(_componentGetExpression, runtimeProperty);
                    _componentGetExpression = Expression.Condition(componentCondition,
                        Expression.Default(_componentGetExpression.Type),
                        _componentGetExpression);
                }

                Expression propertyAccess = Expression.Property(getExpression, runtimeProperty);

                var propertyType = propertyAccess.Type;
                if (propertyType.IsValueType
                    && Nullable.GetUnderlyingType(propertyType) == null)
                {
                    propertyAccess = Expression.Convert(
                        propertyAccess, typeof(Nullable<>).MakeGenericType(propertyType));
                }

                var condition = Expression.Equal(getExpression, Expression.Constant(null, getExpression.Type));

                if (isLastMember)
                {
                    var setMethod = runtimeProperty.SetMethod;
                    if (setMethod == null
                        && runtimeProperty.PropertyType.TryGetSequenceType() == null)
                    {
                        throw new ArgumentException($"The property {runtimeProperty.Name} doesn't have a setter.");
                    }

                    _setExpression = setMethod == null
                        ? null
                        : Expression.Condition(condition,
                            Expression.Default(typeof(void)),
                            Expression.Call(getExpression, setMethod, _valueParameter));
                }

                getExpression = Expression.Condition(condition, Expression.Default(propertyAccess.Type), propertyAccess);

                if (isLastMember)
                {
                    if (getExpression.Type != _resultType)
                    {
                        getExpression = Expression.Convert(getExpression, _resultType);
                    }

                    if (_componentGetExpression != null
                        && _componentGetExpression.Type != _resultType)
                    {
                        _componentGetExpression = Expression.Convert(_componentGetExpression, _resultType);
                    }
                }

                return getExpression;
            }
        }

        public static MethodInfo GetRequiredRuntimeMethod(this Type type, string name, params Type[] parameters)
        {
            var method = type.GetTypeInfo().GetRuntimeMethod(name, parameters);
            if (method == null)
            {
                throw new InvalidOperationException();
            }

            return method;
        }

        public static Type TryGetSequenceType(this Type type)
            => type.TryGetElementType(typeof(IEnumerable<>))
               ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));


        public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            if (type.IsGenericTypeDefinition)
            {
                return null;
            }

            var types = GetGenericTypeImplementations(type, interfaceOrBaseType);

            Type singleImplementation = null;
            foreach (var implementation in types)
            {
                if (singleImplementation == null)
                {
                    singleImplementation = implementation;
                }
                else
                {
                    singleImplementation = null;
                    break;
                }
            }

            return singleImplementation?.GenericTypeArguments.FirstOrDefault();
        }

        public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                var baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                    ? typeInfo.ImplementedInterfaces
                    : type.GetBaseTypes();
                foreach (var baseType in baseTypes)
                {
                    if (baseType.IsGenericType
                        && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                    {
                        yield return baseType;
                    }
                }

                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return type;
                }
            }
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            var currentType = type.BaseType;

            while (currentType != null)
            {
                yield return currentType;

                currentType = currentType.BaseType;
            }
        }

        public static Type UnwrapNullableType(this Type type)
            => Nullable.GetUnderlyingType(type) ?? type;

        public static bool IsNumeric(this Type type)
        {
            type = type.UnwrapNullableType();

            return type.IsInteger()
                || type == typeof(decimal)
                || type == typeof(float)
                || type == typeof(double);
        }

        public static bool IsInteger(this Type type)
        {
            type = type.UnwrapNullableType();

            return type == typeof(int)
                || type == typeof(long)
                || type == typeof(short)
                || type == typeof(byte)
                || type == typeof(uint)
                || type == typeof(ulong)
                || type == typeof(ushort)
                || type == typeof(sbyte)
                || type == typeof(char);
        }

        public static bool IsSignedInteger(this Type type)
            => type == typeof(int)
                || type == typeof(long)
                || type == typeof(short)
                || type == typeof(sbyte);
    }
}

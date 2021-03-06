using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Utils
{
    public static class Extensions
    {
        public static IReadOnlyCollection<T> GetFlags<T>(this T flags)
        {
            var values = new List<T>();
            var defaultValue = Enum.ToObject(typeof(T), value: 0);
            foreach (Enum currValue in Enum.GetValues(typeof(T)))
            {
                if (currValue.Equals(defaultValue))
                {
                    continue;
                }

                if (((Enum)(object)flags).HasFlag(currValue))
                {
                    values.Add((T)(object)currValue);
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
                        values.ExceptWith(decomposedValues.Where(v => !Equals(v, currentValue)));
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

        public static PropertyInfo GetPropertyAccess([NotNull] this LambdaExpression propertyAccessExpression)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var parameterExpression = propertyAccessExpression.Parameters.Single();
            var propertyInfo = parameterExpression.MatchSimplePropertyAccess(propertyAccessExpression.Body);

            if (propertyInfo == null)
            {
                throw new ArgumentException(nameof(propertyAccessExpression));
            }

            var declaringType = propertyInfo.DeclaringType;
            var parameterType = parameterExpression.Type;

            if (declaringType != null
                && declaringType != parameterType
                && declaringType.GetTypeInfo().IsInterface
                && declaringType.GetTypeInfo().IsAssignableFrom(parameterType.GetTypeInfo()))
            {
                var propertyGetter = propertyInfo.GetMethod;
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
            }

            return propertyInfo;
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

        public static MethodInfo GetRequiredRuntimeMethod(this Type type, string name, params Type[] parameters)
        {
            var method = type.GetTypeInfo().GetRuntimeMethod(name, parameters);
            if (method == null)
            {
                throw new InvalidOperationException();
            }

            return method;
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

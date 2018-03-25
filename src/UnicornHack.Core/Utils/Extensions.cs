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

        public static TList With<TList, T>(this TList list, T item) where TList : ICollection<T>
        {
            list.Add(item);
            return list;
        }

        public static TList AddRange<TList, T>(this TList list, IEnumerable<T> items) where TList : ICollection<T>
        {
            foreach (var item in items)
            {
                list.Add(item);
            }

            return list;
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

        public static TQueue EnqueueRange<TQueue, T>(this TQueue queue, IEnumerable<T> items) where TQueue : Queue<T>
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
    }
}

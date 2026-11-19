using System.Linq.Expressions;
using System.Collections.Concurrent;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

public static class RewardGenerationWeight
{
    private static readonly ConcurrentDictionary<string, Func<string, int, int, string, float>> Functions = new(StringComparer.Ordinal);

    public static Func<string, int, int, string, float> CreateFunction(string expression)
        => Functions.GetOrAdd(expression, static weight => new UnicornExpressionVisitor(
        [
            Expression.Parameter(typeof(string), "branch"),
            Expression.Parameter(typeof(int), "depth"),
            Expression.Parameter(typeof(int), "instances"),
            Expression.Parameter(typeof(string), "feature")
        ]).Translate<Func<string, int, int, string, float>, float>(weight));

    public static float Evaluate(ref Func<string, int, int, string, float>? function,
        string expression, string name, RoomComponent room, string feature, int instances = 0)
    {
        try
        {
            function ??= CreateFunction(expression);
            var weight = function(room.Level.Branch.Name, room.Level.Depth, instances, feature);
            return float.IsNaN(weight) || float.IsNegativeInfinity(weight) || weight < 0
                ? throw new InvalidOperationException("Generation weight must be nonnegative and not NaN.")
                : weight;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Error while evaluating the GenerationWeight for " + name, exception);
        }
    }
}
namespace UnicornHack.Utils.DataStructures;

// ReSharper disable once InconsistentNaming
public class AATreeTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Simple(bool allowDuplicates)
    {
        Test(new[] { 1 }, allowDuplicates);

        Test(new[] { 1, 2 }, allowDuplicates);
        Test(new[] { 2, 1 }, allowDuplicates);

        Test(new[] { 1, 2, 3 }, allowDuplicates);
        Test(new[] { 2, 1, 3 }, allowDuplicates);
        Test(new[] { 1, 3, 2 }, allowDuplicates);
        Test(new[] { 2, 3, 1 }, allowDuplicates);
        Test(new[] { 3, 1, 2 }, allowDuplicates);
        Test(new[] { 3, 2, 1 }, allowDuplicates);

        if (allowDuplicates)
        {
            Test(new[] { 1, 1, 2 }, true);
            Test(new[] { 1, 2, 1 }, true);
            Test(new[] { 2, 1, 1 }, true);
            Test(new[] { 2, 2, 1 }, true);
            Test(new[] { 2, 1, 2 }, true);
            Test(new[] { 1, 2, 2 }, true);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Random(bool allowDuplicates)
    {
        var randomCount = 1000;
        var randomCollection = allowDuplicates ? new List<int>(randomCount) : (ICollection<int>)new HashSet<int>();
        var random = new Random(0);
        for (var i = 0; i < randomCount; i++)
        {
            randomCollection.Add(random.Next(allowDuplicates ? randomCount : int.MaxValue));
        }

        Test(randomCollection, allowDuplicates);
    }

    private static void Test(ICollection<int> values, bool allowDuplicates)
    {
        var strictTree = allowDuplicates ? null : new AATreeStrict<int, int>();
        var laxTree = allowDuplicates ? new AATreeLax<int, int>() : null;
        var tree = (AATree<int, int>?)strictTree ?? laxTree!;
        var i = 0;
        var min = int.MaxValue;
        var max = int.MinValue;
        foreach (var value in values)
        {
            Assert.True(tree.Insert(value, ++i), $"Failed to insert {value}");
            if (value < min)
            {
                min = value;
            }

            if (value > max)
            {
                max = value;
            }
        }

        Assert.Equal(min, tree.GetMin().Item1);
        Assert.Equal(max, tree.GetMax().Item1);

        int? smaller = null;
        var previous = int.MinValue;
        foreach (var tuple in tree)
        {
            smaller = tuple.Item1 > previous && previous != int.MinValue
                ? previous
                : smaller;
            Assert.True(tuple.Item1 >= previous, $"Item #{tuple.Item2} with value {tuple.Item1} is not ordered");
            if (smaller != null
                && smaller != tuple.Item1)
            {
                Assert.Equal(smaller, tree.GetNextSmaller(tuple.Item1).Item1);
                if (smaller != max)
                {
                    Assert.Equal(tuple.Item1, tree.GetNextLarger(smaller.Value).Item1);
                }
                else
                {
                    Assert.Throws<InvalidOperationException>(() => tree.GetNextLarger(smaller.Value));
                }
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => tree.GetNextSmaller(tuple.Item1));
                smaller = tuple.Item1;
            }

            previous = tuple.Item1;
        }

        i = 0;
        foreach (var value in values)
        {
            var j = 0;
            foreach (var value2 in values)
            {
                if (j < i)
                {
                    int? result = null;
                    if (allowDuplicates)
                    {
                        result = laxTree!.GetValues(value2).Cast<int?>().SingleOrDefault(v => v == j + 1);
                    }
                    else if (strictTree!.TryGetValue(value2, out var index2))
                    {
                        result = index2;
                    }

                    Assert.True(result == null, $"Found deleted key {value2} with value {result}");
                    Assert.Empty(tree.GetRange(value2, value2).Where(v => v.Item2 == j + 1));
                }
                else
                {
                    if (allowDuplicates)
                    {
                        var found = laxTree!.GetValues(value2).ToList();
                        Assert.Contains(j + 1, found);
                        Assert.Equal(found, tree.GetRange(value2, value2).Select(v => v.Item2));
                    }
                    else
                    {
                        Assert.Equal(j + 1, strictTree![value2]);
                        Assert.Equal(j + 1, tree.GetRange(value2, value2).Single().Item2);
                    }
                }

                j++;
            }

            var removed = allowDuplicates
                ? laxTree!.Remove(value, i + 1)
                : strictTree!.Remove(value);
            Assert.True(removed, $"Failed to delete {value} at {i}");

            i++;
        }
    }
}

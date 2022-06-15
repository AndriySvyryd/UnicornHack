using Xunit;

namespace UnicornHack.Utils;

public class SequenceTest
{
    [Fact]
    public void Alternating()
    {
        Assert.Equal(new[] { 4, 3, 5, 2, 6, 1 }, Sequence.GetAlternating(4, 1, 6));
        Assert.Equal(new[] { -1, 0, 1, 2, 3, 4 }, Sequence.GetAlternating(-1, -1, 4));
        Assert.Equal(new[] { 3, 2, 1, 0 }, Sequence.GetAlternating(3, 0, 3));
    }
}

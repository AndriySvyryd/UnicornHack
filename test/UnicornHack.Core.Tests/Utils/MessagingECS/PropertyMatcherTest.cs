using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyMatcherTest
    {
        [Fact]
        public void Matches()
        {
            var matcher = new PropertyMatcher(1, "foo").With(2, "bar");

            Assert.True(matcher.Matches(2, "bar"));
            Assert.False(matcher.Matches(1, "bar"));
        }
    }
}

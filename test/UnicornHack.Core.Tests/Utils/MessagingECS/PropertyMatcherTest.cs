using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyMatcherTest
    {
        [Fact]
        public void Matches()
        {
            var matcher = new PropertyMatcher().With(c => ((TestComponent)c).Foo, 1).With(c => ((TestComponent)c).Bar, 2);

            Assert.True(matcher.Matches(2, nameof(TestComponent.Bar)));
            Assert.False(matcher.Matches(1, nameof(TestComponent.Bar)));
        }

        private class TestComponent : Component
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }
    }
}

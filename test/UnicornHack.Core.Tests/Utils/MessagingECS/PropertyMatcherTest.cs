using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyMatcherTest
    {
        [Fact]
        public void Matches_none()
        {
            var matcher = new PropertyMatcher<GameEntity>();

            Assert.False(matcher.Matches(1, null));

            Assert.False(matcher.Matches(2, nameof(TestComponent.Foo)));
            Assert.False(matcher.Matches(1, nameof(TestComponent.Bar)));
        }

        [Fact]
        public void Matches_one()
        {
            var matcher = new PropertyMatcher<GameEntity>().With(c => ((TestComponent)c).Bar, 2);

            Assert.True(matcher.Matches(2, null));
            Assert.False(matcher.Matches(1, null));

            Assert.True(matcher.Matches(2, nameof(TestComponent.Bar)));
            Assert.False(matcher.Matches(1, nameof(TestComponent.Bar)));
            Assert.False(matcher.Matches(1, nameof(TestComponent.Foo)));
            Assert.False(matcher.Matches(2, nameof(TestComponent.Foo)));
        }

        [Fact]
        public void Matches_two()
        {
            var matcher = new PropertyMatcher<GameEntity>().With(c => ((TestComponent)c).Foo, 1).With(c => ((TestComponent)c).Bar, 2);

            Assert.True(matcher.Matches(1, null));
            Assert.True(matcher.Matches(2, null));
            Assert.False(matcher.Matches(3, null));

            Assert.True(matcher.Matches(1, nameof(TestComponent.Foo)));
            Assert.False(matcher.Matches(2, nameof(TestComponent.Foo)));
            Assert.True(matcher.Matches(2, nameof(TestComponent.Bar)));
            Assert.False(matcher.Matches(1, nameof(TestComponent.Bar)));
        }

        [Fact]
        public void Matches_three()
        {
            var matcher = new PropertyMatcher<GameEntity>()
                .With(c => ((TestComponent)c).Foo, 1)
                .With(c => ((TestComponent)c).Bar, 2)
                .With(c => ((TestComponent)c).Zom, 3);

            Assert.True(matcher.Matches(1, null));
            Assert.True(matcher.Matches(2, null));
            Assert.True(matcher.Matches(3, null));
            Assert.False(matcher.Matches(4, null));

            Assert.True(matcher.Matches(1, nameof(TestComponent.Foo)));
            Assert.False(matcher.Matches(2, nameof(TestComponent.Foo)));
            Assert.True(matcher.Matches(2, nameof(TestComponent.Bar)));
            Assert.False(matcher.Matches(1, nameof(TestComponent.Bar)));
            Assert.True(matcher.Matches(3, nameof(TestComponent.Zom)));
            Assert.False(matcher.Matches(1, nameof(TestComponent.Zom)));
            Assert.False(matcher.Matches(2, nameof(TestComponent.Zom)));
        }

        private class TestComponent : Component
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Zom { get; set; }
        }
    }
}

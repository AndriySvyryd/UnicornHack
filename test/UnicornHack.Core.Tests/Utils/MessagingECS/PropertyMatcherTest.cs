namespace UnicornHack.Utils.MessagingECS;

public class PropertyMatcherTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Matches_only_registered_component_property_pairs(int propertyCount)
    {
        var matcher = new PropertyMatcher<GameEntity>();
        if (propertyCount >= 1)
        {
            matcher = matcher.With(component => ((TestComponent)component).Foo, 1);
        }
        if (propertyCount >= 2)
        {
            matcher = matcher.With(component => ((TestComponent)component).Bar, 2);
        }
        if (propertyCount >= 3)
        {
            matcher = matcher.With(component => ((TestComponent)component).Zom, 3);
        }

        string[] properties = [nameof(TestComponent.Foo), nameof(TestComponent.Bar), nameof(TestComponent.Zom), "Unknown"];
        for (var componentId = 0; componentId <= 4; componentId++)
        {
            for (var propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
            {
                var expected = propertyIndex < propertyCount && componentId == propertyIndex + 1;
                Assert.Equal(expected, matcher.Matches(componentId, properties[propertyIndex]));
            }
        }
    }

    private class TestComponent : Component
    {
        public string? Foo
        {
            get;
            set;
        }

        public string? Bar
        {
            get;
            set;
        }

        public string? Zom
        {
            get;
            set;
        }
    }
}

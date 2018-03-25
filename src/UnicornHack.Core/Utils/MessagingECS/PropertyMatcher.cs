using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyMatcher
    {
        private readonly List<(int, string)> _properties = new List<(int, string)>();

        public PropertyMatcher(int componentId, string propertyName) => With(componentId, propertyName);

        public PropertyMatcher With(int componentId, string propertyName)
        {
            _properties.Add((componentId, propertyName));
            return this;
        }

        public bool Matches(int componentId, string propertyName)
            => _properties.Contains((componentId, propertyName));
    }
}

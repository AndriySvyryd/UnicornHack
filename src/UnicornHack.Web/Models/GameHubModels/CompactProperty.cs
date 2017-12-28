using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;

namespace UnicornHack.Models.GameHubModels
{
    public static class CompactProperty
    {
        public static List<object> Serialize(Property property, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(2)
                        : new List<object>(3) {(int)state};

                    properties.Add(property.Name);
                    properties.Add(context.Services.Language.ToString(property));
                    return properties;
                case EntityState.Deleted:
                    return new List<object> {(int)state, property.Name};
            }

            properties = new List<object> {(int)state, property.Name};
            var propertyEntry = context.Context.Entry(property);
            var i = 1;

            if (propertyEntry.State != EntityState.Unchanged)
            {
                var value = propertyEntry.Property("_currentValue");
                if (value.IsModified)
                {
                    properties.Add(i);
                    properties.Add(context.Services.Language.ToString(property));
                }
            }

            return properties.Count > 2 ? properties : null;
        }
    }
}
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;

namespace UnicornHack.Models.GameHubModels
{
    public static class CompactConnection
    {
        public static List<object> Serialize(Connection connection, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(4)
                        : new List<object>(5) {state};
                    properties.Add(connection.Id);
                    properties.Add(connection.LevelX);
                    properties.Add(connection.LevelY);
                    properties.Add(connection.TargetLevelDepth > connection.LevelDepth);
                    return properties;
                case EntityState.Deleted:
                    return new List<object>{state,connection.Id};
            }

            var connectionEntry = context.Context.Entry(connection);
            properties = new List<object> {state, connection.Id};

            var i = 1;
            if (connectionEntry.State != EntityState.Unchanged)
            {
                var levelX = connectionEntry.Property(nameof(Connection.LevelX));
                if (levelX.IsModified)
                {
                    properties.Add(i);
                    properties.Add(connection.LevelX);
                }

                i++;
                var levelY = connectionEntry.Property(nameof(Connection.LevelY));
                if (levelY.IsModified)
                {
                    properties.Add(i);
                    properties.Add(connection.LevelY);
                }
            }

            return properties.Count > 2 ? properties : null;
        }
    }
}
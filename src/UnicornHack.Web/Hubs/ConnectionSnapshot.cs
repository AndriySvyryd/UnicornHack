using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs;

public static class ConnectionSnapshot
{
    public static List<object> Serialize(
        GameEntity connectionEntity, EntityState? state, SerializationContext context)
    {
        List<object> properties;
        switch (state)
        {
            case null:
            case EntityState.Added:
            {
                var manager = context.Manager;
                var connectionKnowledge = connectionEntity.Knowledge;
                var knownEntity = connectionKnowledge.KnownEntity;
                var connection = knownEntity.Connection;
                var position = knownEntity.Position;
                properties = state == null
                    ? new List<object>(4)
                    : new List<object>(5) { (int)state };
                properties.Add(connectionEntity.Id);
                properties.Add(position.LevelX);
                properties.Add(position.LevelY);
                properties.Add(manager.FindEntity(connection.TargetLevelId).Level.Depth
                               > position.LevelEntity.Level.Depth);
                return properties;
            }
            case EntityState.Deleted:
                return new List<object> { (int)state, connectionEntity.Id };
            default:
            {
                var manager = context.Manager;
                var connectionKnowledge = connectionEntity.Knowledge;
                var knownEntity = connectionKnowledge.KnownEntity;
                var position = knownEntity.Position;
                var connectionEntry = context.DbContext.Entry(connectionKnowledge);
                properties = new List<object> { (int)state, connectionKnowledge.EntityId };

                var i = 1;
                var positionEntry = context.DbContext.Entry(position);
                if (positionEntry.State != EntityState.Unchanged)
                {
                    var levelX = connectionEntry.Property(nameof(PositionComponent.LevelX));
                    if (levelX.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(position.LevelX);
                    }

                    i++;
                    var levelY = connectionEntry.Property(nameof(PositionComponent.LevelY));
                    if (levelY.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(position.LevelY);
                    }
                }

                return properties.Count > 2 ? properties : null;
            }
        }
    }
}

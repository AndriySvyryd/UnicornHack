using System.Collections;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs;

public static class ConnectionSnapshot
{
    public static List<object?>? Serialize(
        GameEntity connectionEntity, EntityState? state, SerializationContext context)
    {
        List<object?> properties;
        switch (state)
        {
            case null:
            case EntityState.Added:
            {
                var manager = context.Manager;
                var connectionKnowledge = connectionEntity.Knowledge!;
                var knownEntity = connectionKnowledge.KnownEntity;
                var connection = knownEntity.Connection!;
                var position = knownEntity.Position!;
                properties =
                [
                    null,
                    position.LevelX,
                    position.LevelY,
                    manager.FindEntity(connection.TargetLevelId)!.Level!.Depth
                        > position.LevelEntity.Level!.Depth,
                ];
                return properties;
            }
            case EntityState.Deleted:
                return SerializationContext.DeletedBitArray;
            default:
            {
                var connectionKnowledge = connectionEntity.Knowledge!;
                var knownEntity = connectionKnowledge.KnownEntity;
                var position = knownEntity.Position!;
                var connectionEntry = context.DbContext.Entry(connectionKnowledge);

                var i = 0;
                var setValues = new bool[4];
                setValues[i++] = true;
                properties = [null];

                var positionEntry = context.DbContext.Entry(position)!;
                if (positionEntry.State != EntityState.Unchanged)
                {
                    var levelX = connectionEntry.Property(nameof(PositionComponent.LevelX));
                    if (levelX.IsModified)
                    {
                        setValues[i++] = true;
                        properties.Add(position.LevelX);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }

                    var levelY = connectionEntry.Property(nameof(PositionComponent.LevelY));
                    if (levelY.IsModified)
                    {
                        setValues[i++] = true;
                        properties.Add(position.LevelY);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }
                }
                else
                {
                    setValues[i++] = false;
                    setValues[i++] = false;
                }

                setValues[i++] = false;

                if (properties.Count == 1)
                {
                    return null;
                }

                Debug.Assert(i == 4);
                properties[0] = new BitArray(setValues);
                return properties;
            }
        }
    }
}

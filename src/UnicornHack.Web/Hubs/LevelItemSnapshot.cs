using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs
{
    public class LevelItemSnapshot
    {
        private string NameSnapshot { get; set; }

        public LevelItemSnapshot Snapshot(GameEntity itemKnowledgeEntity, SerializationContext context)
        {
            var item = itemKnowledgeEntity.Knowledge.KnownEntity.Item;
            var manager = context.Manager;
            NameSnapshot = context.Services.Language.GetString(item, item.GetQuantity(manager), SenseType.Sight);

            return this;
        }

        public static List<object> Serialize(
            GameEntity knowledgeEntity, EntityState? state, LevelItemSnapshot snapshot, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                {
                    var manager = context.Manager;
                    var item = knowledgeEntity.Knowledge.KnownEntity.Item;
                    var position = knowledgeEntity.Position;
                    properties = state == null
                        ? new List<object>(6)
                        : new List<object>(7) {(int)state};
                    properties.Add(knowledgeEntity.Id);
                    properties.Add((int)item.Type);
                    properties.Add(item.TemplateName);
                    properties.Add(
                        context.Services.Language.GetString(item, item.GetQuantity(manager), SenseType.Sight));
                    properties.Add(position.LevelX);
                    properties.Add(position.LevelY);
                    return properties;
                }
                case EntityState.Deleted:
                    return new List<object>
                    {
                        (int)state,
                        knowledgeEntity.Id
                    };
                default:
                {
                    var manager = context.Manager;
                    var item = knowledgeEntity.Knowledge.KnownEntity.Item;
                    var position = knowledgeEntity.Position;
                    properties = new List<object>(2)
                    {
                        (int)state,
                        knowledgeEntity.Id
                    };

                    var i = 3;
                    var newName = context.Services.Language.GetString(item, item.GetQuantity(manager), SenseType.Sight);
                    if (snapshot.NameSnapshot != newName)
                    {
                        properties.Add(i);
                        properties.Add(newName);
                    }

                    var positionEntry = context.DbContext.Entry(position);
                    if (positionEntry.State != EntityState.Unchanged)
                    {
                        i++;
                        var levelX = positionEntry.Property(nameof(PositionComponent.LevelX));
                        if (levelX.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(position.LevelX);
                        }

                        i++;
                        var levelY = positionEntry.Property(nameof(PositionComponent.LevelY));
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
}

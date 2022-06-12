using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Primitives;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs
{
    public class LevelItemSnapshot
    {
        private bool CurrentlyPerceived { get; set; }
        private string NameSnapshot { get; set; }

        public LevelItemSnapshot CaptureState(GameEntity knowledgeEntity, SerializationContext context)
        {
            var itemKnowledge = knowledgeEntity.Knowledge;
            var knownEntity = itemKnowledge.KnownEntity;
            var position = knowledgeEntity.Position;
            var item = knownEntity.Item;
            var manager = context.Manager;
            CurrentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell).CanIdentify();
            NameSnapshot = itemKnowledge.SensedType.CanIdentify()
                ? context.Services.Language.GetString(item, item.GetQuantity(), itemKnowledge.SensedType)
                : null;

            return this;
        }

        public static List<object> Serialize(
            GameEntity knowledgeEntity, EntityState? state, LevelItemSnapshot snapshot, SerializationContext context)
        {
            List<object> properties;
            var manager = context.Manager;
            var itemKnowledge = knowledgeEntity.Knowledge;
            var knownEntity = itemKnowledge?.KnownEntity;
            var item = knownEntity?.Item;
            var position = knowledgeEntity.Position;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(7)
                        : new List<object>(8) { (int)state };
                    properties.Add(knowledgeEntity.Id);

                    string name = null;
                    if (itemKnowledge.SensedType.CanIdentify())
                    {
                        properties.Add((int)item.Type);
                        properties.Add(item.TemplateName);
                        name = context.Services.Language.GetString(item, item.GetQuantity(), itemKnowledge.SensedType);
                    }
                    else
                    {
                        properties.Add((int)ItemType.None);
                        properties.Add(null);
                    }

                    if (snapshot != null)
                    {
                        snapshot.NameSnapshot = name;
                    }
                    properties.Add(name);

                    properties.Add(position.LevelX);
                    properties.Add(position.LevelY);

                    if (itemKnowledge.SensedType.CanIdentify())
                    {
                        var currentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell).CanIdentify();
                        if (snapshot != null)
                        {
                            snapshot.CurrentlyPerceived = currentlyPerceived;
                        }
                        properties.Add(currentlyPerceived);
                    }

                    return properties;
                case EntityState.Deleted:
                    return new List<object>
                    {
                        (int)state,
                        knowledgeEntity.Id
                    };
                default:
                    properties = new List<object>(2)
                    {
                        (int)state,
                        knowledgeEntity.Id
                    };

                    var i = 1;

                    var knowledgeEntry = context.DbContext.Entry(itemKnowledge);
                    var sensedType = knowledgeEntry.Property(nameof(KnowledgeComponent.SensedType));
                    if (sensedType.IsModified)
                    {
                        var canIdentify = itemKnowledge.SensedType.CanIdentify();
                        properties.Add(i);
                        properties.Add(!canIdentify
                            ? (int)ItemType.None
                            : item.Type);

                        i++;
                        properties.Add(i);
                        properties.Add(!canIdentify
                            ? null
                            : item.TemplateName);
                    }
                    else
                    {
                        i++;
                    }

                    i++;
                    var newName = itemKnowledge.SensedType.CanIdentify()
                        ? context.Services.Language.GetString(item, item.GetQuantity(), itemKnowledge.SensedType)
                        : null;
                    if (snapshot.NameSnapshot != newName)
                    {
                        properties.Add(i);
                        properties.Add(newName);
                        snapshot.NameSnapshot = newName;
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
                    else
                    {
                        i += 2;
                    }

                    {
                        i++;
                        var currentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell).CanIdentify();
                        var currentPerceptionChanged = snapshot.CurrentlyPerceived != currentlyPerceived;
                        if (currentPerceptionChanged)
                        {
                            properties.Add(i);
                            properties.Add(currentlyPerceived);
                            snapshot.CurrentlyPerceived = currentlyPerceived;
                        }
                    }

                    return properties.Count > 2 ? properties : null;
            }
        }
    }
}

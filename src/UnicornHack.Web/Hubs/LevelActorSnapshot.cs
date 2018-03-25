using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs
{
    public static class LevelActorSnapshot
    {
        public static List<object> Serialize(
            GameEntity knowledgeEntity, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                {
                    var actorKnowledge = knowledgeEntity.Knowledge;
                    var knownEntity = actorKnowledge.KnownEntity;
                    var ai = knownEntity.AI;
                    var position = knowledgeEntity.Position;
                    var manager = context.Manager;
                    properties = state == null
                        ? new List<object>(6)
                        : new List<object>(7) {(int)state};
                    properties.Add(actorKnowledge.EntityId);
                    // TODO: Move to language service and take sense into account
                    properties.Add(ai != null
                        ? manager.RacesToBeingRelationship[actorKnowledge.KnownEntityId].Values.First().Race
                            .TemplateName
                        : "player");
                    properties.Add(ai != null
                        ? ai.ProperName
                        : knownEntity.Player.ProperName);
                    properties.Add(position.LevelX);
                    properties.Add(position.LevelY);
                    properties.Add((byte)position.Heading);
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
                    var actorKnowledge = knowledgeEntity.Knowledge;
                    var knownEntity = actorKnowledge.KnownEntity;
                    var ai = knownEntity.AI;
                    var position = knowledgeEntity.Position;
                    var manager = context.Manager;
                    properties = new List<object>(2)
                    {
                        (int)state,
                        actorKnowledge.EntityId
                    };

                    var actorEntry = ai != null
                        ? (EntityEntry)context.DbContext.Entry(ai)
                        : context.DbContext.Entry(knownEntity.Player);
                    var i = 2;
                    var name = actorEntry.Property(nameof(AIComponent.ProperName));
                    if (name.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(ai != null
                            ? ai.ProperName
                            : knownEntity.Player.ProperName);
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

                        i++;
                        var heading = positionEntry.Property(nameof(PositionComponent.Heading));
                        if (heading.IsModified)
                        {
                            properties.Add(i);
                            properties.Add((byte)position.Heading);
                        }
                    }

                    return properties.Count > 2 ? properties : null;
                }
            }
        }
    }
}

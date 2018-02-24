using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;

namespace UnicornHack.Models.GameHubModels
{
    public static class CompactActor
    {
        public static List<object> Serialize(ActorKnowledge actorKnowledge, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(6)
                        : new List<object>(7) {(int)state};
                    properties.Add(actorKnowledge.Id);
                    properties.Add(actorKnowledge.Actor.VariantName);
                    properties.Add(actorKnowledge.Actor.Name);
                    properties.Add(actorKnowledge.LevelX);
                    properties.Add(actorKnowledge.LevelY);
                    properties.Add((byte)actorKnowledge.Heading);
                    return properties;
                case EntityState.Deleted:
                    return new List<object> {(int)state, actorKnowledge.Id};
            }

            properties = new List<object>(2) {(int)state, actorKnowledge.Id};

            var actorKnowledgeEntry = context.Context.Entry(actorKnowledge);
            var actorEntry = context.Context.Entry(actorKnowledge.Actor);
            var i = 2;
            if (actorKnowledgeEntry.State != EntityState.Unchanged)
            {
                var name = actorEntry.Property(nameof(Actor.Name));
                if (name.IsModified)
                {
                    properties.Add(i);
                    properties.Add(actorKnowledge.Actor.Name);
                }

                i++;
                var levelX = actorKnowledgeEntry.Property(nameof(Actor.LevelX));
                if (levelX.IsModified)
                {
                    properties.Add(i);
                    properties.Add(actorKnowledge.LevelX);
                }

                i++;
                var levelY = actorKnowledgeEntry.Property(nameof(Actor.LevelY));
                if (levelY.IsModified)
                {
                    properties.Add(i);
                    properties.Add(actorKnowledge.LevelY);
                }

                i++;
                var heading = actorKnowledgeEntry.Property(nameof(Actor.Heading));
                if (heading.IsModified)
                {
                    properties.Add(i);
                    properties.Add((byte)actorKnowledge.Heading);
                }
            }

            return properties.Count > 2 ? properties : null;
        }
    }
}
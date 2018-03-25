using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Hubs
{
    public static class RaceSnapshot
    {
        public static List<object> Serialize(
            GameEntity raceEntity, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                {
                    var race = raceEntity.Race;
                    properties = state == null
                        ? new List<object>(5)
                        : new List<object>(6) {(int)state};
                    properties.Add(raceEntity.Id);
                    properties.Add(race.TemplateName);
                    properties.Add(race.Level);
                    properties.Add(race.ExperiencePoints);
                    properties.Add(race.NextLevelXP);
                    return properties;
                }
                case EntityState.Deleted:
                    return new List<object>
                    {
                        (int)state,
                        raceEntity.Id
                    };
                default:
                {
                    var race = raceEntity.Race;
                    properties = new List<object>
                    {
                        (int)state,
                        raceEntity.Id
                    };

                    var raceEntry = context.DbContext.Entry(race);
                    var i = 1;

                    if (raceEntry.State != EntityState.Unchanged)
                    {
                        var name = raceEntry.Property(nameof(RaceComponent.TemplateName));
                        if (name.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(race.TemplateName);
                        }

                        i++;
                        var xpLevel = raceEntry.Property(nameof(RaceComponent.Level));
                        if (xpLevel.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(race.Level);
                        }

                        i++;
                        var xp = raceEntry.Property(nameof(RaceComponent.ExperiencePoints));
                        if (xp.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(race.ExperiencePoints);
                        }

                        i++;
                        var nextLevelXP = raceEntry.Property(nameof(RaceComponent.NextLevelXP));
                        if (nextLevelXP.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(race.NextLevelXP);
                        }
                    }

                    return properties.Count > 2 ? properties : null;
                }
            }
        }
    }
}

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Hubs;

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
                    ? new List<object>(4)
                    : new List<object>(5) { (int)state };
                properties.Add(raceEntity.Id);
                properties.Add(context.Services.Language.GetString(race, abbreviate: false));
                properties.Add(context.Services.Language.GetString(race, abbreviate: true));
                properties.Add(race.Level);
                return properties;
            }
            case EntityState.Deleted:
                return new List<object> { (int)state, raceEntity.Id };
            default:
            {
                var race = raceEntity.Race;
                properties = new List<object> { (int)state, raceEntity.Id };

                var raceEntry = context.DbContext.Entry(race);
                var i = 1;

                if (raceEntry.State != EntityState.Unchanged)
                {
                    var name = raceEntry.Property(nameof(RaceComponent.TemplateName));
                    if (name.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(context.Services.Language.GetString(race, abbreviate: false));

                        i++;
                        properties.Add(i);
                        properties.Add(context.Services.Language.GetString(race, abbreviate: true));
                    }
                    else
                    {
                        i++;
                    }

                    i++;
                    var xpLevel = raceEntry.Property(nameof(RaceComponent.Level));
                    if (xpLevel.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(race.Level);
                    }
                }

                return properties.Count > 2 ? properties : null;
            }
        }
    }
}

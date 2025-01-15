using System.Collections;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Hubs;

public static class RaceSnapshot
{
    public static List<object?>? Serialize(
        GameEntity raceEntity, EntityState? state, SerializationContext context)
    {
        List<object?> properties;
        switch (state)
        {
            case null:
            case EntityState.Added:
            {
                var race = raceEntity.Race!;
                properties =
                [
                    null,
                    context.Services.Language.GetString(race, abbreviate: false),
                    context.Services.Language.GetString(race, abbreviate: true)
                ];
                return properties;
            }
            case EntityState.Deleted:
                return SerializationContext.DeletedBitArray;
            default:
            {
                var race = raceEntity.Race!;
                var raceEntry = context.DbContext.Entry(race);
                if (raceEntry.State == EntityState.Unchanged)
                {
                    return null;
                }

                var i = 0;
                var setValues = new bool[3];
                setValues[i++] = true;
                properties = [null];

                var name = raceEntry.Property(nameof(RaceComponent.TemplateName));
                if (name.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add(context.Services.Language.GetString(race, abbreviate: false));

                    setValues[i++] = true;
                    properties.Add(context.Services.Language.GetString(race, abbreviate: true));
                }
                else
                {
                    setValues[i++] = false;
                    setValues[i++] = false;
                }

                Debug.Assert(i == 3);
                if (properties.Count == 1)
                {
                    return null;
                }

                properties[0] = new BitArray(setValues);
                return properties;
            }
        }
    }
}

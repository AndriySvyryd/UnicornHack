using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;
using UnicornHack.Effects;

namespace UnicornHack.Models.GameHubModels
{
    public static class CompactPlayerRace
    {
        public static List<object> Serialize(AppliedEffect raceEffect, EntityState? state, SerializationContext context)
        {
            var race = (ChangedRace)raceEffect;
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                {
                    properties = state == null
                        ? new List<object>(5)
                        : new List<object>(6) {(int)state};
                    properties.Add(race.Id);
                    properties.Add(race.Name);
                    properties.Add(race.XPLevel);
                    properties.Add(race.XP);
                    properties.Add(race.NextLevelXP);
                    return properties;
                }
                case EntityState.Deleted:
                    return new List<object> {(int)state, race.Id};
            }

            properties = new List<object> {(int)state, race.Id};

            var raceEntry = context.Context.Entry(race);
            var i = 1;

            if (raceEntry.State != EntityState.Unchanged)
            {
                var name = raceEntry.Property(nameof(ChangedRace.Name));
                if (name.IsModified)
                {
                    properties.Add(i);
                    properties.Add(race.Name);
                }

                i++;
                var xpLevel = raceEntry.Property(nameof(ChangedRace.XPLevel));
                if (xpLevel.IsModified)
                {
                    properties.Add(i);
                    properties.Add(race.XPLevel);
                }

                i++;
                var xp = raceEntry.Property(nameof(ChangedRace.XP));
                if (xp.IsModified)
                {
                    properties.Add(i);
                    properties.Add(race.XP);
                }

                i++;
                var nextLevelXP = raceEntry.Property(nameof(ChangedRace.NextLevelXP));
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
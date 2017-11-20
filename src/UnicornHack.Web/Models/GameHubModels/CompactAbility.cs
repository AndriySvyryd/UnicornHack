using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;

namespace UnicornHack.Models.GameHubModels
{
    public static class CompactAbility
    {
        public static List<object> Serialize(Ability ability, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                {
                    var canBeDefault = CanBeDefault(ability);
                    {
                        properties = state == null
                            ? new List<object>(canBeDefault ? 3 : 2)
                            : new List<object>(canBeDefault ? 4 : 3) {state};
                    }

                    properties.Add(ability.Id);
                    properties.Add(context.Services.Language.ToString(ability));
                    if (canBeDefault
                        && ability.Entity is Player addedPlayer)
                    {
                        properties.Add(addedPlayer.DefaultAttack == ability);
                    }

                    return properties;
                }
                case EntityState.Deleted:
                    return new List<object> {state, ability.Id};
            }

            properties = new List<object> {state, ability.Id};
            var abilityEntry = context.Context.Entry(ability);
            var i = 1;
            var name = abilityEntry.Property(nameof(Ability.Name));
            if (name.IsModified)
            {
                properties.Add(i);
                properties.Add(context.Services.Language.ToString(ability));
            }

            i++;
            if (CanBeDefault(ability)
                && ability.Entity is Player player)
            {
                var defaultAttack = abilityEntry.Context.Entry(player).Reference(nameof(Player.DefaultAttack));
                if (defaultAttack.IsModified)
                {
                    properties.Add(i);
                    properties.Add(player.DefaultAttack == ability);
                }
            }

            return properties.Count > 2 ? properties : null;
        }

        public static void Snapshot(Ability ability)
        {
        }

        private static bool CanBeDefault(Ability ability)
        {
            switch (ability.Name)
            {
                case Actor.DoubleMeleeAttackName:
                case Actor.PrimaryMeleeAttackName:
                case Actor.SecondaryMeleeAttackName:
                case Actor.DoubleRangedAttackName:
                case Actor.PrimaryRangedAttackName:
                case Actor.SecondaryRangedAttackName:
                    return true;
                default:
                    return false;
            }
        }
    }
}
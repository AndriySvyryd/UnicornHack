using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Abilities;

namespace UnicornHack.Hubs
{
    public static class AbilitySnapshot
    {
        public static List<object> Serialize(GameEntity abilityEntity, EntityState? state, SerializationContext context)
        {
            List<object> properties;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    {
                        var ability = abilityEntity.Ability;
                        var player = context.Observer.Player;
                        var canBeDefault = context.Manager.SkillAbilitiesSystem.CanBeDefaultAttack(ability);
                        {
                            properties = state == null
                                ? new List<object>(4)
                                : new List<object>(5) { (int)state };
                        }

                        properties.Add(abilityEntity.Id);
                        properties.Add(context.Services.Language.GetString(ability));
                        properties.Add(abilityEntity.Ability.Activation);
                        properties.Add(abilityEntity.Ability.Slot);

                        return properties;
                    }
                case EntityState.Deleted:
                    return new List<object>
                    {
                        (int)state,
                        abilityEntity.Id
                    };
                default:
                    {
                        var ability = abilityEntity.Ability;
                        var player = context.Observer.Player;
                        properties = new List<object>
                            {
                                (int)state,
                                abilityEntity.Id
                            };
                        var abilityEntry = context.DbContext.Entry(ability);
                        var i = 1;
                        var name = abilityEntry.Property(nameof(AbilityComponent.Name));
                        if (name.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(context.Services.Language.GetString(ability));
                        }

                        i++;
                        var activation = abilityEntry.Property(nameof(AbilityComponent.Activation));
                        if (activation.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(abilityEntity.Ability.Activation);
                        }

                        i++;
                        var slot = abilityEntry.Property(nameof(AbilityComponent.Slot));
                        if (slot.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(abilityEntity.Ability.Slot);
                        }

                        return properties.Count > 2 ? properties : null;
                    }
            }
        }
    }
}

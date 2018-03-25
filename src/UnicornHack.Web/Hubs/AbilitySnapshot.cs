using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;

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
                            ? new List<object>(canBeDefault ? 3 : 2)
                            : new List<object>(canBeDefault ? 4 : 3) {(int)state};
                    }

                    properties.Add(abilityEntity.Id);
                    properties.Add(context.Services.Language.GetString(ability));
                    if (canBeDefault)
                    {
                        properties.Add(player.DefaultAttackId == abilityEntity.Id);
                    }

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
                    if (context.Manager.SkillAbilitiesSystem.CanBeDefaultAttack(ability))
                    {
                        var defaultAttack =
                            abilityEntry.Context.Entry(player).Property(nameof(PlayerComponent.DefaultAttackId));
                        if (defaultAttack.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(player.DefaultAttackId == ability.EntityId);
                        }
                    }

                    return properties.Count > 2 ? properties : null;
                }
            }
        }
    }
}

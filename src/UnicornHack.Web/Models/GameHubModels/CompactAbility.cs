using UnicornHack.Data;
using UnicornHack.Services;

namespace UnicornHack.Models.GameHubModels
{
    public class CompactAbility
    {
        public object[] Properties { get; set; }

        public static CompactAbility Serialize(Ability ability, GameDbContext context, GameServices services)
        {
            var canBeDefault = ability.Name == Actor.DoubleMeleeAttackName
                               || ability.Name == Actor.PrimaryMeleeAttackName
                               || ability.Name == Actor.SecondaryMeleeAttackName
                               || ability.Name == Actor.DoubleRangedAttackName
                               || ability.Name == Actor.PrimaryRangedAttackName
                               || ability.Name == Actor.SecondaryRangedAttackName;

            var properties = new object[canBeDefault ? 3 : 2];
            var i = 0;
            properties[i++] = ability.Id;
            properties[i++] = services.Language.ToString(ability);
            if (canBeDefault
                && ability.Entity is Player player)
            {
                properties[i] = player.DefaultAttack == ability;
            }

            return new CompactAbility
            {
                Properties = properties
            };
        }
    }
}
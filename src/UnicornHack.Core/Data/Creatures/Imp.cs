using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Imp = new Creature
        {
            Name = "imp",
            Species = Species.Imp,
            SpeciesClass = SpeciesClass.Demon,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Claw,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+PerceptionModifier()",
                    Cooldown = 100,
                    Delay = "100*SpeedModifier()",
                    Effects = new List<Effect> {new PhysicalDamage {Damage = "20*MightModifier()"}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = "5",
            Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
            Noise = ActorNoiseType.Cuss,
            Size = 1,
            Weight = 100,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -8,
            Regeneration = 3,
            Armor = 4,
            MagicResistance = 10,
            LocomotionType = LocomotionType.Flying,
            Infravisible = true,
            Infravision = true
        };
    }
}

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Mane = new Creature
        {
            Name = "mane",
            Species = Species.Homunculus,
            SpeciesClass = SpeciesClass.Demon,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Claw,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "20*physicalScaling"}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            GenerationFlags = GenerationFlags.LargeGroup,
            Behavior = AIBehavior.Stalking,
            Noise = ActorNoiseType.Hiss,
            Weight = 500,
            MovementDelay = 300,
            TurningDelay = 300,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -4,
            Armor = 1,
            Infravisible = true,
            Infravision = true
        };
    }
}

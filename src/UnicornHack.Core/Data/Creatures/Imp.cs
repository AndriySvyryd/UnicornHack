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
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 7F},
            Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
            Noise = ActorNoiseType.Cuss,
            Size = 1,
            Weight = 100,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
            Regeneration = 3,
            Armor = 4,
            MagicResistance = 10,
            LocomotionType = LocomotionType.Flying,
            Infravisible = true,
            Infravision = true
        };
    }
}

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Firefly = new Creature
        {
            Name = "firefly",
            Species = Species.Beetle,
            SpeciesClass = SpeciesClass.Vermin,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Sting,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new Burn {Damage = "10*physicalScaling"}}
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Noise = ActorNoiseType.Buzz,
            Size = 1,
            Weight = 10,
            Perception = -9,
            Might = -9,
            Speed = -5,
            Focus = -9,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            Infravisible = true
        };
    }
}

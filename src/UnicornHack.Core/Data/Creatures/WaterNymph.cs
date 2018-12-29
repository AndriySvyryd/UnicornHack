using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature WaterNymph = new Creature
        {
            Name = "water nymph",
            Species = Species.Nymph,
            SpeciesClass = SpeciesClass.Fey,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Sedate {Duration = 2}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted, Action = AbilityAction.Touch, Cooldown = 100, Effects =
                        new HashSet<Effect>
                        {
                            new StealItem()
                        }
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Seduction,
            Sex = Sex.Female,
            Weight = 600,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
            MagicDeflection = 10,
            PhysicalDeflection = 11,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            LocomotionType = LocomotionType.Swimming,
            Infravisible = true
        };
    }
}

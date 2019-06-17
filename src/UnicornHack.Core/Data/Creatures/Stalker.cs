using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Stalker = new Creature
        {
            Name = "stalker",
            Species = Species.Elemental,
            SpeciesClass = SpeciesClass.Extraplanar,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Claw,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "100*physicalScaling"}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
            Size = 8,
            Weight = 900,
            Perception = -5,
            Might = -6,
            Speed = -5,
            Focus = -6,
            Armor = 3,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.Claws,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            NoiseLevel = 0,
            VisibilityLevel = 0,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

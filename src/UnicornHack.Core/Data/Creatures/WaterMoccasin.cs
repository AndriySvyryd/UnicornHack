using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature WaterMoccasin = new Creature
        {
            Name = "water moccasin",
            Species = Species.Snake,
            SpeciesClass = SpeciesClass.Reptile,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new Blight {Damage = "30*physicalScaling"}}
                }
            },
            InitialLevel = 4,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.LargeGroup,
            Noise = ActorNoiseType.Hiss,
            Size = 2,
            Weight = 150,
            MovementDelay = -20,
            TurningDelay = -20,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -8,
            Armor = 3,
            TorsoType = TorsoType.Serpentine,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 0,
            VisibilityLevel = 2,
            Infravision = true
        };
    }
}

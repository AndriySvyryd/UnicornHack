using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature PitViper = new Creature
        {
            Name = "pit viper",
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
                    Effects = new HashSet<Effect> {new Blight {Damage = "50*physicalScaling"}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Noise = ActorNoiseType.Hiss,
            Weight = 100,
            MovementDelay = -20,
            TurningDelay = -20,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -6,
            Armor = 4,
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

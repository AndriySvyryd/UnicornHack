using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BabyFairyDragon = new Creature
        {
            Name = "baby fairy dragon",
            Species = Species.Dragon,
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "70*physicalScaling"}}
                }
            },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "fairy dragon",
            Noise = ActorNoiseType.Roar,
            Size = 8,
            Weight = 1500,
            MovementDelay = 33,
            TurningDelay = 33,
            Perception = -3,
            Might = -4,
            Speed = -3,
            Focus = -4,
            Armor = 4,
            MagicResistance = 5,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 1,
            VisibilityLevel = 0,
            Infravision = true
        };
    }
}

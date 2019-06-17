using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BabyCrocodile = new Creature
        {
            Name = "baby crocodile",
            Species = Species.Crocodile,
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "60*physicalScaling"}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "crocodile",
            Size = 2,
            Weight = 200,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -6,
            Armor = 2,
            UpperExtremities = ExtremityType.None,
            RespirationType = RespirationType.Amphibious,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 1
        };
    }
}

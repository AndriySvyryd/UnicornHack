using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature RockPiercer = new Creature
        {
            Name = "rock piercer",
            Species = Species.Piercer,
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
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Size = 2,
            Weight = 200,
            MovementDelay = 1100,
            TurningDelay = 1100,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -8,
            Armor = 3,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            InventorySize = 0,
            EyeCount = 0,
            NoiseLevel = 0,
            VisibilityLevel = 1,
            Clingy = true
        };
    }
}

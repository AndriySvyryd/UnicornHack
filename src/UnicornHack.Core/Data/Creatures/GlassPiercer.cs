using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GlassPiercer = new Creature
        {
            Name = "glass piercer",
            Species = Species.Piercer,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "140*physicalScaling"}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Size = 2,
            Weight = 400,
            MovementDelay = 1100,
            TurningDelay = 1100,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -6,
            Armor = 4,
            AcidResistance = 75,
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

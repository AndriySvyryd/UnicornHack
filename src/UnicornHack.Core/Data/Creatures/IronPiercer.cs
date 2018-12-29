using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature IronPiercer = new Creature
        {
            Name = "iron piercer",
            Species = Species.Piercer,
            MovementDelay = 1200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Size = 2,
            Weight = 300,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            PhysicalDeflection = 18,
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

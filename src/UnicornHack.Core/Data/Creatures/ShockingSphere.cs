using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature ShockingSphere = new Creature
        {
            Name = "shocking sphere",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 92,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Explosion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Shock {Damage = 140}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Sex = Sex.None,
            Size = 2,
            Weight = 10,
            Perception = 4,
            Might = 4,
            Speed = 4,
            Focus = 4,
            MagicDeflection = 5,
            PhysicalDeflection = 16,
            ElectricityResistance = 75,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}

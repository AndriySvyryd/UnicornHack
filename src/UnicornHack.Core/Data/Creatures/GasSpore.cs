using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GasSpore = new Creature
        {
            Name = "gas spore",
            Species = Species.FloatingSphere,
            MovementDelay = 400,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Explosion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 140}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Explosion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Deafen {Duration = 27}}
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Sex = Sex.None,
            Size = 2,
            Weight = 10,
            Perception = 1,
            Might = 0,
            Speed = 1,
            Focus = 6,
            PhysicalDeflection = 10,
            SlimingImmune = true,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            EyeCount = 0,
            NoiseLevel = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}

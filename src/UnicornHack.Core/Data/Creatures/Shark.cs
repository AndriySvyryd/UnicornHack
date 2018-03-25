using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Shark = new Creature
        {
            Name = "shark",
            Species = Species.Fish,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 170}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            Size = 8,
            Weight = 1000,
            Agility = 4,
            Constitution = 4,
            Intelligence = 4,
            Quickness = 4,
            Strength = 4,
            Willpower = 4,
            PhysicalDeflection = 18,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.None,
            RespirationType = RespirationType.Water,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 0
        };
    }
}

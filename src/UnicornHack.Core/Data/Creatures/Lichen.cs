using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Lichen = new Creature
        {
            Name = "lichen",
            Species = Species.Fungus,
            MovementDelay = 1200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect>
                    {
                        new Stick()
                    }
                },
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeHit,
                    Effects = new HashSet<Effect>
                    {
                        new Stick()
                    }
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            Sex = Sex.None,
            Size = 2,
            Weight = 20,
            Agility = 1,
            Constitution = 1,
            Intelligence = 1,
            Quickness = 1,
            Strength = 1,
            Willpower = 6,
            PhysicalDeflection = 11,
            HeadType = HeadType.None,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.None,
            RespirationType = RespirationType.None,
            InventorySize = 0,
            EyeCount = 0,
            NoiseLevel = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}

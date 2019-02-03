using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature FreezingSphere = new Creature
        {
            Name = "freezing sphere",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 92,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 20,
                    Action = AbilityAction.Explosion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Freeze {Damage = 140}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 4F}, Name = "hell"},
            Sex = Sex.None,
            Size = 2,
            Weight = 10,
            Perception = 4,
            Might = 4,
            Speed = 4,
            Focus = 4,
            Armor = 3,
            MagicResistance = 5,
            ColdResistance = 75,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            Infravisible = true,
            Mindless = true,
            NonAnimal = true
        };
    }
}

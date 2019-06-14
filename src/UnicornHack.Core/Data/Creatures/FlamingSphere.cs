using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature FlamingSphere = new Creature
        {
            Name = "flaming sphere",
            Species = Species.FloatingSphere,
            SpeciesClass = SpeciesClass.Extraplanar,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 20,
                    Action = AbilityAction.Explosion,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new Burn {Damage = "140*mentalScaling"}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 4F}, Name = "hell"},
            Sex = Sex.None,
            Size = 2,
            Weight = 10,
            MovementDelay = -8,
            TurningDelay = -8,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -6,
            Armor = 3,
            MagicResistance = 5,
            FireResistance = 75,
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

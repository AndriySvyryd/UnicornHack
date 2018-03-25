using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Juiblex = new Creature
        {
            Name = "Juiblex",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 400,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Engulf {Duration = 20}}
                },
                new Ability
                {
                    Activation = ActivationType.OnDigestion,
                    Action = AbilityAction.Digestion,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 220}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spit,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 100}}
                }
            },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight
            {
                Matched = new DefaultWeight {Multiplier = 0F},
                Name = "hell"
            },
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.RangedPeaceful | AIBehavior.Stalking,
            Noise = ActorNoiseType.Gurgle,
            Sex = Sex.Male,
            Size = 8,
            Weight = 1500,
            Agility = 16,
            Constitution = 16,
            Intelligence = 16,
            Quickness = 16,
            Strength = 16,
            Willpower = 16,
            MagicResistance = 65,
            PhysicalDeflection = 27,
            AcidResistance = 75,
            FireResistance = 75,
            StoningImmune = true,
            HeadType = HeadType.None,
            TorsoType = TorsoType.Amorphic,
            RespirationType = RespirationType.Amphibious,
            LocomotionType = LocomotionType.Flying,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

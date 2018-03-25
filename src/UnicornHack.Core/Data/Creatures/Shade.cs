using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Shade = new Creature
        {
            Name = "shade",
            Species = Species.Ghost,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 120,
            Material = Material.Air,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Paralyze {Duration = 7}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Slow {Duration = 3}}
                }
            },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            PreviousStageName = "ghost",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
            Noise = ActorNoiseType.Howl,
            Weight = 0,
            Agility = 7,
            Constitution = 7,
            Intelligence = 7,
            Quickness = 7,
            Strength = 7,
            Willpower = 12,
            MagicResistance = 25,
            PhysicalDeflection = 10,
            ColdResistance = 75,
            DisintegrationResistance = 75,
            SlimingImmune = true,
            StoningImmune = true,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Walking | LocomotionType.Phasing,
            InventorySize = 0,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

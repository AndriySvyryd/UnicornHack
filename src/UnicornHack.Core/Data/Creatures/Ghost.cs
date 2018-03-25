using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Ghost = new Creature
        {
            Name = "ghost",
            Species = Species.Ghost,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 400,
            Material = Material.Air,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            InitialLevel = 10,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            NextStageName = "shade",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
            Weight = 0,
            Agility = 6,
            Constitution = 6,
            Intelligence = 6,
            Quickness = 6,
            Strength = 6,
            Willpower = 11,
            MagicResistance = 15,
            PhysicalDeflection = 25,
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
            Infravision = true
        };
    }
}

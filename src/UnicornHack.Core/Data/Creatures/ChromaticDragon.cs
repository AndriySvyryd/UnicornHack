using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature ChromaticDragon = new Creature
        {
            Name = "Chromatic Dragon",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Breath,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ElementalDamage"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 180}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                }
            },
            InitialLevel = 16,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.RangedPeaceful | AIBehavior.Stalking | AIBehavior.GoldCollector |
                       AIBehavior.GemCollector,
            Noise = ActorNoiseType.Roar,
            Sex = Sex.Female,
            Size = 32,
            Weight = 4500,
            Agility = 9,
            Constitution = 9,
            Intelligence = 9,
            Quickness = 9,
            Strength = 9,
            Willpower = 14,
            MagicResistance = 30,
            AcidResistance = 75,
            ColdResistance = 75,
            ElectricityResistance = 75,
            FireResistance = 75,
            StoningImmune = true,
            TorsoType = TorsoType.Quadruped,
            UpperExtremeties = ExtremityType.None,
            LowerExtremeties = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

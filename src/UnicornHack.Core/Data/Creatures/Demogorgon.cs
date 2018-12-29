using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Demogorgon = new Creature
        {
            Name = "Demogorgon",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 80,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spell,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Sting,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new DrainLife {Amount = 2}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new ChangeRace {RaceName = "zombie", Delay = 10000}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new ChangeRace {RaceName = "zombie", Delay = 10000}}
                }
            },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 0F}, Name = "hell"},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Stalking,
            Noise = ActorNoiseType.Growl,
            Sex = Sex.Male,
            Size = 8,
            Weight = 1500,
            Perception = 16,
            Might = 16,
            Speed = 16,
            Focus = 16,
            MagicDeflection = 47,
            PhysicalDeflection = 28,
            FireResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            LocomotionType = LocomotionType.Flying,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

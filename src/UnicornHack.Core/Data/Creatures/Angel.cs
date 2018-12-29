using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Angel = new Creature
        {
            Name = "angel",
            Species = Species.Angel,
            SpeciesClass = SpeciesClass.Celestial,
            MovementDelay = 120,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spell,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Disintegrate {Damage = 70}}
                }
            },
            InitialLevel = 14,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 4F}, Name = "hell"},
            PreviousStageName = "aleax",
            NextStageName = "archon",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.AlignmentAware | AIBehavior.Stalking | AIBehavior.WeaponCollector |
                       AIBehavior.MagicUser,
            Noise = ActorNoiseType.Speach,
            Weight = 1000,
            Perception = 8,
            Might = 8,
            Speed = 8,
            Focus = 12,
            MagicDeflection = 27,
            PhysicalDeflection = 24,
            ColdResistance = 75,
            ElectricityResistance = 75,
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

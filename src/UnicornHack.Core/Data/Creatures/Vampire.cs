using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Vampire = new Creature
        {
            Name = "vampire",
            Species = Species.Vampire,
            SpeciesClass = SpeciesClass.ShapeChanger | SpeciesClass.Undead,
            MovementDelay = 100,
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
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new DrainLife {Amount = 3}}
                }
            },
            InitialLevel = 10,
            NextStageName = "vampire lord",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach,
            Weight = 1000,
            Perception = 6,
            Might = 6,
            Speed = 6,
            Focus = 10,
            Regeneration = 3,
            MagicDeflection = 12,
            PhysicalDeflection = 19,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            Infravision = true
        };
    }
}

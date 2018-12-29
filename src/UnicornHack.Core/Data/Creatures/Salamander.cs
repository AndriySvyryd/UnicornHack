using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Salamander = new Creature
        {
            Name = "salamander",
            Species = Species.Salamander,
            SpeciesClass = SpeciesClass.Extraplanar,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 90}}
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
                    Action = AbilityAction.Touch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Burn {Damage = 30}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Hug,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Burn {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Hug,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Burn {Damage = 100}}
                }
            },
            InitialLevel = 10,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 2F}, Name = "hell"},
            Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector | AIBehavior.MagicUser,
            Noise = ActorNoiseType.Mumble,
            Size = 8,
            Weight = 1500,
            Perception = 6,
            Might = 6,
            Speed = 6,
            Focus = 10,
            PhysicalDeflection = 21,
            FireResistance = 75,
            SlimingImmune = true,
            TorsoType = TorsoType.Serpentine,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            Infravisible = true,
            Infravision = true
        };
    }
}

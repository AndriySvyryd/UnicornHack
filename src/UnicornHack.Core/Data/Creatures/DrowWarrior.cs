using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature DrowWarrior = new Creature
        {
            Name = "drow warrior",
            Species = Species.Elf,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "50"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "10*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "10*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                        {new Sedate {Duration = EffectDuration.UntilTimeout, DurationAmount = "5"}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.SmallGroup,
            Behavior = AIBehavior.AlignmentAware | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach,
            Weight = 800,
            MovementDelay = 100,
            Perception = 4,
            Might = 4,
            Speed = 4,
            Focus = 8,
            MagicResistance = 25,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

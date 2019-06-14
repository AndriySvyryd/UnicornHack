using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature LurkerAbove = new Creature
        {
            Name = "lurker above",
            Species = Species.Trapper,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                        {new Engulf {Duration = EffectDuration.UntilTimeout, DurationAmount = "4"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnDigestion,
                    Action = AbilityAction.Digestion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect>
                    {
                        new Suffocate()
                    }
                }
            },
            InitialLevel = 10,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = AIBehavior.Stalking,
            Size = 8,
            Weight = 800,
            MovementDelay = 300,
            TurningDelay = 300,
            Perception = -4,
            Might = -4,
            Speed = -4,
            Focus = -4,
            Armor = 3,
            HeadType = HeadType.None,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            LocomotionType = LocomotionType.Flying,
            EyeCount = 0,
            NoiseLevel = 0,
            VisibilityLevel = 1,
            Clingy = true
        };
    }
}

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Giant = new Creature
        {
            Name = "giant",
            Species = Species.Giant,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "110"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "10*physicalScaling"}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable,
            Behavior = AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Boast,
            Size = 16,
            Weight = 2250,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -6,
            Armor = 2,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

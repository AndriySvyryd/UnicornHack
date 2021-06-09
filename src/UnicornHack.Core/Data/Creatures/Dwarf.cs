using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Dwarf = new Creature
        {
            Name = "dwarf",
            Species = Species.Dwarf,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = ActivationType.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "30"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Punch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "10*MightModifier()"}}
                    }
                },
            InitialLevel = 2,
            GenerationWeight = "0",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior =
                AIBehavior.AlignmentAware | AIBehavior.GoldCollector | AIBehavior.GemCollector |
                AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach,
            Weight = 900,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -8,
            MagicResistance = 5,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            LocomotionType = LocomotionType.Walking | LocomotionType.ToolTunneling,
            Infravisible = true,
            Infravision = true
        };
    }
}

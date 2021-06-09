using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature AnimatedSkeleton = new Creature
        {
            Name = "animated skeleton",
            Species = Species.Skeleton,
            SpeciesClass = SpeciesClass.Undead,
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
            InitialLevel = 3,
            GenerationWeight = "3",
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = AIBehavior.WeaponCollector,
            Weight = 300,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -4,
            PhysicalResistance = 20,
            ColdResistance = 75,
            StoningImmune = true,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Infravision = true,
            Mindless = true
        };
    }
}

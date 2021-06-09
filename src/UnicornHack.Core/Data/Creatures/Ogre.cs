using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Ogre = new Creature
        {
            Name = "ogre",
            Species = Species.Ogre,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = ActivationType.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "60"}}
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
            InitialLevel = 5,
            GenerationWeight = "3",
            NextStageName = "ogre lord",
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt,
            Size = 8,
            Weight = 1600,
            MovementDelay = 20,
            TurningDelay = 20,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -8,
            Armor = 2,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true
        };
    }
}

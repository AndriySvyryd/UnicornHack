using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature FrostGiant = new Creature
        {
            Name = "frost giant",
            Species = Species.Giant,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = ActivationType.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new List<Effect> {new PhysicalDamage {Damage = "110"}}
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
            InitialLevel = 10,
            GenerationWeight = "$branch == 'hell' ? 0 : 1",
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Boast,
            Size = 16,
            Weight = 2250,
            Perception = -4,
            Might = -4,
            Speed = -4,
            Focus = -4,
            Armor = 3,
            MagicResistance = 5,
            ColdResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

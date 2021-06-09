using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Lich = new Creature
        {
            Name = "lich",
            Species = Species.Lich,
            SpeciesClass = SpeciesClass.Undead,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnMeleeAttack,
                        Range = 1,
                        Action = AbilityAction.Touch,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new Freeze {Damage = "50*MightModifier()"}}
                    },
                    new Ability
                    {
                        Activation = ActivationType.Targeted,
                        Trigger = ActivationType.OnRangedAttack,
                        Range = 20,
                        Action = AbilityAction.Spell,
                        SuccessCondition = AbilitySuccessCondition.NormalAttack,
                        Accuracy = "5+PerceptionModifier()",
                        Cooldown = 100,
                        Delay = "100*SpeedModifier()",
                        Effects = new List<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                    }
                },
            InitialLevel = 11,
            NextStageName = "demilich",
            Behavior = AIBehavior.MagicUser,
            Noise = ActorNoiseType.Mumble,
            Weight = 600,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -4,
            Might = -4,
            Speed = -4,
            Regeneration = 3,
            MagicResistance = 15,
            ColdResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Infravision = true
        };
    }
}

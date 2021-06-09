using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Werewolf = new Creature
        {
            Name = "werewolf",
            Species = Species.Human,
            SpeciesClass = SpeciesClass.ShapeChanger,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new List<Effect> {new PhysicalDamage {Damage = "50"}}
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
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+PerceptionModifier()",
                    Cooldown = 250,
                    Delay = "100*SpeedModifier()",
                    Effects = new List<Effect> {new ConferLycanthropy {VariantName = "wolfwere"}}
                }
            },
            InitialLevel = 5,
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Lycanthrope,
            Weight = 1000,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -8,
            Regeneration = 3,
            MagicResistance = 10,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Lycanthropy = "wolfwere"
        };
    }
}

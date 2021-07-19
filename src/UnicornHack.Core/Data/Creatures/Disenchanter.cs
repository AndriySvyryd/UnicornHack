using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Disenchanter = new Creature
        {
            Name = "disenchanter",
            Species = Species.Disenchanter,
            Abilities = new HashSet<Ability>
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
                    Effects = new List<Effect> {new DrainEnergy {Amount = "10*FocusModifier()"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnMeleeHit,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                    Accuracy = "10+PerceptionModifier()",
                    Effects = new List<Effect> {new DrainEnergy {Amount = "5*FocusModifier()"}}
                }
            },
            InitialLevel = 12,
            GenerationWeight = "2",
            Noise = ActorNoiseType.Growl,
            Weight = 750,
            Perception = -3,
            Might = -4,
            Speed = -3,
            Focus = -4,
            Armor = 10,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            SlotCapacity = 1,
            Infravisible = true,
            Infravision = true
        };
    }
}

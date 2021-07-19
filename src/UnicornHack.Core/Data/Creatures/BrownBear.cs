using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BrownBear = new Creature
        {
            Name = "brown bear",
            Species = Species.Bear,
            SpeciesClass = SpeciesClass.Quadrupedal,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+PerceptionModifier()",
                    Cooldown = 100,
                    Delay = "100*SpeedModifier()",
                    Effects = new List<Effect> {new PhysicalDamage {Damage = "20*MightModifier()"}}
                }
            },
            InitialLevel = 10,
            GenerationWeight = "3",
            Noise = ActorNoiseType.Bark,
            Size = 8,
            Weight = 300,
            MovementDelay = -20,
            TurningDelay = -20,
            Perception = -2,
            Might = 3,
            Speed = -3,
            Focus = -6,
            Armor = 1,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            SlotCapacity = 1,
            Infravisible = true
        };
    }
}

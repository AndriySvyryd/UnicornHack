using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature DireWolf = new Creature
        {
            Name = "dire wolf",
            Species = Species.Wolf,
            SpeciesClass = SpeciesClass.Canine,
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
                    Effects = new List<Effect> {new PhysicalDamage {Damage = "70*MightModifier()"}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = "3",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Bark,
            Weight = 1200,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -6,
            Armor = 3,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            SlotCapacity = 1,
            Infravisible = true
        };
    }
}

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BloodthirstyRat = new Creature
        {
            Name = "bloodthirsty rat",
            Species = Species.Rat,
            SpeciesClass = SpeciesClass.Rodent,
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
            InitialLevel = 2,
            GenerationWeight = "5",
            PreviousStageName = "sewer rat",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Sqeek,
            Size = 2,
            Weight = 150,
            MovementDelay = 20,
            TurningDelay = 20,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -8,
            Armor = 1,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            SlotCapacity = 1,
            Infravisible = true
        };
    }
}

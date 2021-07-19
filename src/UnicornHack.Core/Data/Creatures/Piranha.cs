using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Piranha = new Creature
        {
            Name = "piranha",
            Species = Species.Fish,
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
            InitialLevel = 5,
            GenerationFlags = GenerationFlags.SmallGroup,
            Size = 1,
            Weight = 60,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -8,
            Armor = 3,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.Water,
            LocomotionType = LocomotionType.Swimming,
            SlotCapacity = 0
        };
    }
}

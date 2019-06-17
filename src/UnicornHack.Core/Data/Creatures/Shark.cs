using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Shark = new Creature
        {
            Name = "shark",
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
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "170*physicalScaling"}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            Size = 8,
            Weight = 1000,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -6,
            Armor = 4,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.Water,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 0
        };
    }
}

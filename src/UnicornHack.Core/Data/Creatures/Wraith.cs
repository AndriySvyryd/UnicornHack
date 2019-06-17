using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Wraith = new Creature
        {
            Name = "wraith",
            Species = Species.Wraith,
            SpeciesClass = SpeciesClass.Undead,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new DrainLife {Amount = "10*mentalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnMeleeHit,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                    Accuracy = "10+attackScaling",
                    Effects = new HashSet<Effect> {new DrainLife {Amount = "5*mentalScaling"}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = AIBehavior.Stalking,
            Noise = ActorNoiseType.Howl,
            Weight = 0,
            Material = Material.Air,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -2,
            Armor = 3,
            MagicResistance = 7,
            ColdResistance = 75,
            SlimingImmune = true,
            StoningImmune = true,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 0,
            Infravision = true
        };
    }
}

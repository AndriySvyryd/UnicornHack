using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature VampireLord = new Creature
        {
            Name = "vampire lord",
            Species = Species.Vampire,
            SpeciesClass = SpeciesClass.ShapeChanger | SpeciesClass.Undead,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "40"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "15*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new DrainLife {Amount = "8*physicalScaling"}}
                }
            },
            InitialLevel = 12,
            PreviousStageName = "vampire",
            NextStageName = "vampire mage",
            Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Hiss,
            Sex = Sex.Male,
            Weight = 1000,
            MovementDelay = 85,
            Perception = 7,
            Might = 6,
            Speed = 7,
            Focus = 12,
            Regeneration = 3,
            MagicResistance = 20,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            LocomotionType = LocomotionType.Flying,
            Infravision = true
        };
    }
}

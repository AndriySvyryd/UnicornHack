using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GiantZombie = new Creature
        {
            Name = "giant zombie",
            Species = Species.Giant,
            SpeciesClass = SpeciesClass.Undead,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "110*physicalScaling"}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = AIBehavior.Stalking,
            Noise = ActorNoiseType.Moan,
            Size = 16,
            Weight = 2250,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -5,
            Might = -6,
            Speed = -5,
            Armor = 3,
            ColdResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Infravision = true,
            Mindless = true
        };
    }
}

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature AcidBlob = new Creature
        {
            Name = "acid blob",
            Species = Species.Blob,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "splashback",
                    Activation = ActivationType.OnMeleeHit,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                    Accuracy = "10+attackScaling",
                    Effects = new HashSet<Effect> {new Corrode {Damage = "40*mentalScaling"}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Behavior = AIBehavior.Wandering,
            Sex = Sex.None,
            Size = 1,
            Weight = 30,
            MovementDelay = 300,
            TurningDelay = 50,
            Perception = -9,
            Might = -5,
            Speed = -9,
            Focus = -4,
            Armor = 1,
            AcidResistance = 75,
            StoningImmune = true,
            HeadType = HeadType.None,
            TorsoType = TorsoType.Amorphic,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            EyeCount = 0,
            NoiseLevel = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}

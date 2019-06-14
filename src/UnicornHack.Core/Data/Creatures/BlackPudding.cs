using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BlackPudding = new Creature
        {
            Name = "black pudding",
            Species = Species.Pudding,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new Corrode {Damage = "130*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnMeleeHit, Effects = new HashSet<Effect> {new Corrode {Damage = "30"}}
                }
            },
            InitialLevel = 10,
            Sex = Sex.None,
            Weight = 512,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -4,
            Might = -4,
            Speed = -4,
            Armor = 2,
            AcidResistance = 75,
            ColdResistance = 75,
            ElectricityResistance = 75,
            StoningImmune = true,
            HeadType = HeadType.None,
            TorsoType = TorsoType.Amorphic,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            EyeCount = 0,
            NoiseLevel = 0,
            Mindless = true,
            NonAnimal = true,
            Reanimation = true
        };
    }
}

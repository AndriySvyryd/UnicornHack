using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature ClayGolem = new Creature
        {
            Name = "clay golem",
            Species = Species.Golem,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "160*physicalScaling"}}
                }
            },
            InitialLevel = 11,
            Sex = Sex.None,
            Size = 8,
            Weight = 1500,
            MovementDelay = 171,
            Material = Material.Mineral,
            Perception = 6,
            Might = 6,
            Speed = 6,
            Focus = 10,
            Armor = 1,
            MagicResistance = 20,
            ElectricityResistance = 75,
            SlimingImmune = true,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Mindless = true,
            NonAnimal = true
        };
    }
}

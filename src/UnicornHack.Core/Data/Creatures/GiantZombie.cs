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
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 110}}
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = AIBehavior.Stalking,
            Noise = ActorNoiseType.Moan,
            Size = 16,
            Weight = 2250,
            Agility = 5,
            Constitution = 5,
            Intelligence = 5,
            Quickness = 5,
            Strength = 5,
            Willpower = 10,
            PhysicalDeflection = 16,
            ColdResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Infravision = true,
            Mindless = true
        };
    }
}

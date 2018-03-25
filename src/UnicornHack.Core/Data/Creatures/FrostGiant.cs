using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature FrostGiant = new Creature
        {
            Name = "frost giant",
            Species = Species.Giant,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 110}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            InitialLevel = 10,
            GenerationWeight = new BranchWeight
            {
                NotMatched = new DefaultWeight {Multiplier = 2F},
                Name = "hell"
            },
            GenerationFlags = GenerationFlags.SmallGroup,
            Behavior = AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Boast,
            Size = 16,
            Weight = 2250,
            Agility = 6,
            Constitution = 6,
            Intelligence = 6,
            Quickness = 6,
            Strength = 6,
            Willpower = 6,
            MagicResistance = 10,
            PhysicalDeflection = 17,
            ColdResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

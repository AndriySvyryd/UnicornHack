using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Erinyes = new Creature
        {
            Name = "erinyes",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Blight {Damage = 10}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new BranchWeight
            {
                Matched = new DefaultWeight {Multiplier = 2F},
                Name = "hell"
            },
            NextStageName = "vrock",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable |
                              GenerationFlags.SmallGroup,
            Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector,
            Weight = 1000,
            Agility = 4,
            Constitution = 4,
            Intelligence = 4,
            Quickness = 4,
            Strength = 4,
            Willpower = 4,
            MagicResistance = 30,
            PhysicalDeflection = 18,
            FireResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

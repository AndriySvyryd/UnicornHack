using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Yeenoghu = new Creature
        {
            Name = "Yeenoghu",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 66,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Confuse {Duration = 9}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Paralyze {Duration = 3}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new MagicalDamage {Damage = 70}}
                }
            },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight
            {
                Matched = new DefaultWeight {Multiplier = 0F},
                Name = "hell"
            },
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector | AIBehavior.MagicUser,
            Noise = ActorNoiseType.Gurgle,
            Sex = Sex.Male,
            Size = 8,
            Weight = 1500,
            Agility = 16,
            Constitution = 16,
            Intelligence = 16,
            Quickness = 16,
            Strength = 16,
            Willpower = 16,
            MagicResistance = 80,
            PhysicalDeflection = 25,
            FireResistance = 75,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

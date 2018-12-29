using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Hezrou = new Creature
        {
            Name = "hezrou",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 2F}, Name = "hell"},
            NextStageName = "marilith",
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.SmallGroup,
            Behavior = AIBehavior.Stalking,
            Size = 8,
            Weight = 1600,
            Perception = 4,
            Might = 4,
            Speed = 4,
            Focus = 4,
            MagicDeflection = 27,
            PhysicalDeflection = 22,
            FireResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

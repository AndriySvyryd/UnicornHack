using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Baalzebub = new Creature
        {
            Name = "Baalzebub",
            Species = Species.DemonMajor,
            SpeciesClass = SpeciesClass.Demon,
            MovementDelay = 133,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Blight {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Gaze,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new Stun {Duration = 7}}
                }
            },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight
            {
                Matched = new DefaultWeight {Multiplier = 0F},
                Name = "hell"
            },
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.RangedPeaceful | AIBehavior.Stalking,
            Noise = ActorNoiseType.Cuss,
            Sex = Sex.Male,
            Size = 8,
            Weight = 1500,
            Agility = 16,
            Constitution = 16,
            Intelligence = 16,
            Quickness = 16,
            Strength = 16,
            Willpower = 16,
            MagicResistance = 85,
            PhysicalDeflection = 25,
            FireResistance = 75,
            LocomotionType = LocomotionType.Flying,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

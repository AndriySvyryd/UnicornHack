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
                    Range = 1,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Blight {Damage = 70}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 20,
                    Action = AbilityAction.Gaze,
                    Cooldown = 100,
                    Effects = new HashSet<Effect>
                        {new Stun {Duration = EffectDuration.UntilTimeout, DurationAmount = 7}}
                }
            },
            InitialLevel = 30,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 0F}, Name = "hell"},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.RangedPeaceful | AIBehavior.Stalking,
            Noise = ActorNoiseType.Cuss,
            Sex = Sex.Male,
            Size = 8,
            Weight = 1500,
            Perception = 16,
            Might = 16,
            Speed = 16,
            Focus = 16,
            Armor = 7,
            MagicResistance = 42,
            FireResistance = 75,
            LocomotionType = LocomotionType.Flying,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Ghast = new Creature
        {
            Name = "ghast",
            Species = Species.Ghoul,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 150,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect>
                        {new Paralyze {Duration = EffectDuration.UntilTimeout, DurationAmount = 3}}
                }
            },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "ghoul",
            Noise = ActorNoiseType.Growl,
            Weight = 400,
            Perception = 8,
            Might = 8,
            Speed = 8,
            Focus = 12,
            Armor = 4,
            ColdResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Infravision = true
        };
    }
}

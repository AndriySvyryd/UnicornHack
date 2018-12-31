using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature PurpleWorm = new Creature
        {
            Name = "purple worm",
            Species = Species.Worm,
            SpeciesClass = SpeciesClass.Vermin,
            MovementDelay = 133,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 90}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Engulf { Duration = EffectDuration.UntilTimeout, DurationAmount = 7}}
                },
                new Ability
                {
                    Activation = ActivationType.OnDigestion,
                    Action = AbilityAction.Digestion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 50}}
                }
            },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "baby purple worm",
            Size = 32,
            Weight = 1500,
            Perception = 8,
            Might = 8,
            Speed = 8,
            Focus = 8,
            MagicDeflection = 10,
            PhysicalDeflection = 15,
            TorsoType = TorsoType.Serpentine,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            InventorySize = 0,
            EyeCount = 0
        };
    }
}

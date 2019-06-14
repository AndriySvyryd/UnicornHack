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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "90*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                        {new Engulf {Duration = EffectDuration.UntilTimeout, DurationAmount = "7"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnDigestion,
                    Action = AbilityAction.Digestion,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Corrode {Damage = "50"}}
                }
            },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "baby purple worm",
            Size = 32,
            Weight = 1500,
            MovementDelay = 33,
            TurningDelay = 33,
            Perception = -2,
            Might = -2,
            Speed = -2,
            Focus = -2,
            Armor = 2,
            MagicResistance = 10,
            TorsoType = TorsoType.Serpentine,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            InventorySize = 0,
            EyeCount = 0
        };
    }
}

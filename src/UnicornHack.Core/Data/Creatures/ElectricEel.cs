using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature ElectricEel = new Creature
        {
            Name = "electric eel",
            Species = Species.Eel,
            MovementDelay = 120,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Shock {Damage = 140}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Hug,
                    Cooldown = 100,
                    Effects = new HashSet<Effect>
                        {new Bind {Duration = EffectDuration.UntilTimeout, DurationAmount = 10}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            PreviousStageName = "giant eel",
            Size = 8,
            Weight = 600,
            Perception = 4,
            Might = 4,
            Speed = 4,
            Focus = 4,
            Armor = 6,
            ElectricityResistance = 75,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.Water,
            LocomotionType = LocomotionType.Swimming,
            InventorySize = 0
        };
    }
}

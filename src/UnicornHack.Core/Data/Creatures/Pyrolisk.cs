using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Pyrolisk = new Creature
        {
            Name = "pyrolisk",
            Species = Species.Cockatrice,
            SpeciesClass = SpeciesClass.MagicalBeast,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 20,
                    Action = AbilityAction.Gaze,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new Burn {Damage = "70*mentalScaling"}}
                }
            },
            InitialLevel = 6,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Noise = ActorNoiseType.Hiss,
            Size = 2,
            Weight = 30,
            MovementDelay = 200,
            Perception = 4,
            Might = 4,
            Speed = 4,
            Focus = 4,
            Armor = 2,
            MagicResistance = 15,
            FireResistance = 75,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}

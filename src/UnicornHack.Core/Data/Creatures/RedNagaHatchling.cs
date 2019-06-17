using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature RedNagaHatchling = new Creature
        {
            Name = "red naga hatchling",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "20*physicalScaling"}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            NextStageName = "red naga",
            Noise = ActorNoiseType.Hiss,
            Weight = 500,
            MovementDelay = 20,
            TurningDelay = 20,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -8,
            Armor = 2,
            FireResistance = 75,
            SlimingImmune = true,
            TorsoType = TorsoType.Serpentine,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            InventorySize = 1,
            Infravision = true
        };
    }
}

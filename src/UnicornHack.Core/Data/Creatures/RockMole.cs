using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature RockMole = new Creature
        {
            Name = "rock mole",
            Species = Species.Mole,
            SpeciesClass = SpeciesClass.Rodent,
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "30*physicalScaling"}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector,
            Size = 2,
            Weight = 100,
            MovementDelay = 300,
            TurningDelay = 300,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -8,
            MagicResistance = 10,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Walking | LocomotionType.Tunneling,
            InventorySize = 1,
            Infravisible = true
        };
    }
}

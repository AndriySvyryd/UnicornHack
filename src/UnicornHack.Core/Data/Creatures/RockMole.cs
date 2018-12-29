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
            MovementDelay = 400,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector,
            Size = 2,
            Weight = 100,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
            MagicDeflection = 10,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Walking | LocomotionType.Tunneling,
            InventorySize = 1,
            Infravisible = true
        };
    }
}

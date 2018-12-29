using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BlueDragon = new Creature
        {
            Name = "blue dragon",
            Species = Species.Dragon,
            SpeciesClass = SpeciesClass.Reptile,
            MovementDelay = 133,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Breath,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Shock {Damage = 140}}
                },
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
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                }
            },
            InitialLevel = 15,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "baby blue dragon",
            Behavior = AIBehavior.Mountable | AIBehavior.GoldCollector | AIBehavior.GemCollector,
            Noise = ActorNoiseType.Roar,
            Size = 32,
            Weight = 4500,
            Perception = 8,
            Might = 8,
            Speed = 8,
            Focus = 8,
            MagicDeflection = 10,
            PhysicalDeflection = 21,
            ElectricityResistance = 75,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 1,
            Infravision = true,
            InvisibilityDetection = true
        };
    }
}

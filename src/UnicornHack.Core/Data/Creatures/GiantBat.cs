using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GiantBat = new Creature
        {
            Name = "giant bat",
            Species = Species.Bat,
            SpeciesClass = SpeciesClass.Bird,
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
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "bat",
            NextStageName = "vampire bat",
            Behavior = AIBehavior.Wandering,
            Noise = ActorNoiseType.Sqeek,
            Size = 1,
            Weight = 100,
            MovementDelay = -46,
            TurningDelay = -46,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -8,
            Armor = 1,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            LocomotionType = LocomotionType.Flying,
            InventorySize = 1,
            NoiseLevel = 0,
            Infravisible = true
        };
    }
}

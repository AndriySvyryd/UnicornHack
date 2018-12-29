using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature VampireBat = new Creature
        {
            Name = "vampire bat",
            Species = Species.Bat,
            SpeciesClass = SpeciesClass.Bird,
            MovementDelay = 60,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect>
                        {new ChangeProperty<int> {PropertyName = "Might", Value = -1, Duration = 5}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            PreviousStageName = "giant bat",
            Behavior = AIBehavior.Wandering,
            Noise = ActorNoiseType.Sqeek,
            Size = 1,
            Weight = 100,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            Regeneration = 3,
            PhysicalDeflection = 14,
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

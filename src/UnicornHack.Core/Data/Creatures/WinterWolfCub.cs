using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature WinterWolfCub = new Creature
        {
            Name = "winter wolf cub",
            Species = Species.Wolf,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Breath,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Freeze {Damage = 40}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new BranchWeight {NotMatched = new DefaultWeight {Multiplier = 5F}, Name = "hell"},
            NextStageName = "winter wolf",
            Noise = ActorNoiseType.Bark,
            Size = 2,
            Weight = 250,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            PhysicalDeflection = 16,
            ColdResistance = 75,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}

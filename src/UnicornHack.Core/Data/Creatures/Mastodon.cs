using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Mastodon = new Creature
        {
            Name = "mastodon",
            Species = Species.Quadruped,
            SpeciesClass = SpeciesClass.Quadrupedal,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Headbutt,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 180}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Headbutt,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 180}}
                }
            },
            InitialLevel = 20,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Noise = ActorNoiseType.Roar,
            Size = 8,
            Weight = 3800,
            Perception = 11,
            Might = 10,
            Speed = 11,
            Focus = 10,
            PhysicalDeflection = 15,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}

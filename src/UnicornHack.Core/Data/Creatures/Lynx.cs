using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Lynx = new Creature
        {
            Name = "lynx",
            Species = Species.Cat,
            SpeciesClass = SpeciesClass.Feline,
            MovementDelay = 80,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Claw,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            Noise = ActorNoiseType.Growl,
            Weight = 400,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            PhysicalDeflection = 14,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}

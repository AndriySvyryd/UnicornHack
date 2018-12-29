using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Cockatrice = new Creature
        {
            Name = "cockatrice",
            Species = Species.Cockatrice,
            SpeciesClass = SpeciesClass.MagicalBeast,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 20}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted, Action = AbilityAction.Touch, Cooldown = 100, Effects =
                        new HashSet<Effect>
                        {
                            new Stone()
                        }
                },
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeHit, Effects = new HashSet<Effect>
                    {
                        new Stone()
                    }
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "chickatrice",
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Hiss,
            Size = 2,
            Weight = 30,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            MagicDeflection = 15,
            PhysicalDeflection = 14,
            StoningImmune = true,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}

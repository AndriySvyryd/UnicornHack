using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GuardianNaga = new Creature
        {
            Name = "guardian naga",
            Species = Species.Naga,
            SpeciesClass = SpeciesClass.Aberration,
            MovementDelay = 75,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spit,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Blight {Damage = 140}}
                }
            },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "guardian naga hatchling",
            Noise = ActorNoiseType.Hiss,
            Size = 16,
            Weight = 1500,
            Perception = 7,
            Might = 6,
            Speed = 7,
            Focus = 6,
            MagicDeflection = 25,
            TorsoType = TorsoType.Serpentine,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            InventorySize = 1,
            Infravision = true
        };
    }
}

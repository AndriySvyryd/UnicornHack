using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature HellHound = new Creature
        {
            Name = "hell hound",
            Species = Species.Dog,
            SpeciesClass = SpeciesClass.Canine,
            MovementDelay = 85,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 100}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 20,
                    Action = AbilityAction.Breath,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Burn {Damage = 100}}
                }
            },
            InitialLevel = 12,
            GenerationWeight = new BranchWeight {Matched = new DefaultWeight {Multiplier = 6F}, Name = "hell"},
            PreviousStageName = "hell hound pup",
            Noise = ActorNoiseType.Bark,
            Weight = 700,
            Perception = 7,
            Might = 6,
            Speed = 7,
            Focus = 6,
            Armor = 4,
            MagicResistance = 10,
            FireResistance = 75,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}

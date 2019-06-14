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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "20*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                    {
                        new Stone()
                    }
                },
                new Ability
                {
                    Activation = ActivationType.OnMeleeHit, Effects = new HashSet<Effect>
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
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -8,
            Armor = 2,
            MagicResistance = 15,
            StoningImmune = true,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}

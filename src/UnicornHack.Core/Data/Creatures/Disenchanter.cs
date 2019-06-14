using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Disenchanter = new Creature
        {
            Name = "disenchanter",
            Species = Species.Disenchanter,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new DrainEnergy {Amount = "10*mentalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnMeleeHit,
                    Effects = new HashSet<Effect> {new DrainEnergy {Amount = "5*mentalScaling"}}
                }
            },
            InitialLevel = 12,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Noise = ActorNoiseType.Growl,
            Weight = 750,
            Perception = -3,
            Might = -4,
            Speed = -3,
            Focus = -4,
            Armor = 10,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true,
            Infravision = true
        };
    }
}

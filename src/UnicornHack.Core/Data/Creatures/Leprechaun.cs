using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Leprechaun = new Creature
        {
            Name = "leprechaun",
            Species = Species.Leprechaun,
            SpeciesClass = SpeciesClass.Fey,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Claw,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                    {
                        new StealGold()
                    }
                }
            },
            InitialLevel = 5,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            Behavior = AIBehavior.GoldCollector,
            Noise = ActorNoiseType.Laugh,
            Size = 1,
            Weight = 60,
            MovementDelay = -20,
            TurningDelay = -20,
            Perception = -7,
            Might = -8,
            Speed = -7,
            Focus = -8,
            Armor = 1,
            MagicResistance = 10,
            LocomotionType = LocomotionType.Walking | LocomotionType.Teleportation,
            Infravisible = true
        };
    }
}

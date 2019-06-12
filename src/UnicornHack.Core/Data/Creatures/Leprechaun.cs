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
                    Range = 1,
                    Action = AbilityAction.Claw,
                    SuccessCondition = AbilitySuccessCondition.Attack,
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
            MovementDelay = 80,
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            Armor = 1,
            MagicResistance = 10,
            LocomotionType = LocomotionType.Walking | LocomotionType.Teleportation,
            Infravisible = true
        };
    }
}

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
            MovementDelay = 80,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted, Action = AbilityAction.Claw, Cooldown = 100, Effects =
                        new HashSet<Effect>
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
            Perception = 3,
            Might = 2,
            Speed = 3,
            Focus = 2,
            MagicDeflection = 10,
            PhysicalDeflection = 12,
            LocomotionType = LocomotionType.Walking | LocomotionType.Teleportation,
            Infravisible = true
        };
    }
}

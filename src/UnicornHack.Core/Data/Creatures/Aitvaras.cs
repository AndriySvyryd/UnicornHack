using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Aitvaras = new Creature
        {
            Name = "aitvaras",
            Species = Species.Aitvaras,
            SpeciesClass = SpeciesClass.Bird,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Gaze,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+PerceptionModifier()",
                    Cooldown = 100,
                    Delay = "100*SpeedModifier()",
                    Effects = new List<Effect> {new Burn {Damage = "70*FocusModifier()"}}
                }
            },
            InitialLevel = 6,
            Noise = ActorNoiseType.Hiss,
            Size = 2,
            Weight = 30,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -6,
            Armor = 2,
            MagicResistance = 15,
            FireResistance = 75,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            SlotCapacity = 1,
            Infravisible = true
        };
    }
}

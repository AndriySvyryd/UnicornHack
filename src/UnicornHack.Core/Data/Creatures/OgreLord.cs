using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature OgreLord = new Creature
        {
            Name = "ogre lord",
            Species = Species.Ogre,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "70"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "10*physicalScaling"}}
                }
            },
            InitialLevel = 7,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "ogre",
            NextStageName = "ogre king",
            Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Grunt,
            Sex = Sex.Male,
            Size = 8,
            Weight = 1650,
            Perception = -6,
            Might = -6,
            Speed = -6,
            Focus = -6,
            Armor = 3,
            MagicResistance = 15,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature KoboldShaman = new Creature
        {
            Name = "kobold shaman",
            Species = Species.Kobold,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spell,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = AIBehavior.WeaponCollector | AIBehavior.MagicUser,
            Noise = ActorNoiseType.Grunt,
            Sex = Sex.Male,
            Size = 2,
            Weight = 450,
            Agility = 2,
            Constitution = 2,
            Intelligence = 2,
            Quickness = 2,
            Strength = 2,
            Willpower = 2,
            MagicResistance = 10,
            PhysicalDeflection = 12,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

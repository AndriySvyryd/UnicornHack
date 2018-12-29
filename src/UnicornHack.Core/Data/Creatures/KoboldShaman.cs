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
                    Cooldown = 100,
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
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
            MagicDeflection = 5,
            PhysicalDeflection = 12,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

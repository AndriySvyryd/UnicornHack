using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Lich = new Creature
        {
            Name = "lich",
            Species = Species.Lich,
            SpeciesClass = SpeciesClass.Undead,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Freeze {Damage = 50}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Spell,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new ScriptedEffect {Script = "ArcaneSpell"}}
                }
            },
            InitialLevel = 11,
            NextStageName = "demilich",
            Behavior = AIBehavior.MagicUser,
            Noise = ActorNoiseType.Mumble,
            Weight = 600,
            Perception = 6,
            Might = 6,
            Speed = 6,
            Focus = 10,
            Regeneration = 3,
            MagicDeflection = 15,
            ColdResistance = 75,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Infravision = true
        };
    }
}

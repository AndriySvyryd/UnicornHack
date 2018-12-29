using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GnomishWizard = new Creature
        {
            Name = "gnomish wizard",
            Species = Species.Gnome,
            MovementDelay = 150,
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
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector | AIBehavior.MagicUser,
            Noise = ActorNoiseType.Speach,
            Sex = Sex.Male,
            Size = 2,
            Weight = 700,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
            MagicDeflection = 10,
            PhysicalDeflection = 10,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Gnome = new Creature
        {
            Name = "gnome",
            Species = Species.Gnome,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            InitialLevel = 1,
            NextStageName = "gnome lord",
            GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.SmallGroup,
            Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach,
            Size = 2,
            Weight = 650,
            Perception = 1,
            Might = 0,
            Speed = 1,
            Focus = 0,
            MagicDeflection = 2,
            PhysicalDeflection = 10,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}

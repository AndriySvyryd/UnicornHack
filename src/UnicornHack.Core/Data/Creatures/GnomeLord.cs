using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GnomeLord = new Creature
        {
            Name = "gnome lord",
            Species = Species.Gnome,
            MovementDelay = 150,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            PreviousStageName = "gnome",
            NextStageName = "gnome king",
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach,
            Sex = Sex.Male,
            Size = 2,
            Weight = 700,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
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

using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Wererat = new Creature
        {
            Name = "wererat",
            Species = Species.Human,
            SpeciesClass = SpeciesClass.ShapeChanger,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Bite,
                    Timeout = 5,
                    Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "ratwere"}}
                }
            },
            InitialLevel = 3,
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Lycanthrope,
            Weight = 1000,
            Agility = 2,
            Constitution = 2,
            Intelligence = 2,
            Quickness = 2,
            Strength = 2,
            Willpower = 2,
            Regeneration = 3,
            MagicResistance = 10,
            PhysicalDeflection = 10,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            Infravisible = true,
            Lycanthropy = "ratwere"
        };
    }
}

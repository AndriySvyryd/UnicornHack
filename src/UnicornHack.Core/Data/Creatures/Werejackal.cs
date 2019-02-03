using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Werejackal = new Creature
        {
            Name = "werejackal",
            Species = Species.Human,
            SpeciesClass = SpeciesClass.ShapeChanger,
            MovementDelay = 100,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Action = AbilityAction.Modifier,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 50}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 10}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Bite,
                    Cooldown = 250,
                    Effects = new HashSet<Effect> {new ConferLycanthropy {VariantName = "jackalwere"}}
                }
            },
            InitialLevel = 2,
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Lycanthrope,
            Weight = 1000,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 2,
            Regeneration = 3,
            MagicResistance = 5,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Lycanthropy = "jackalwere"
        };
    }
}

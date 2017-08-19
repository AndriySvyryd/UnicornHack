using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant Bugbear = new CreatureVariant
        {
            Name = "bugbear",
            Species = Species.Bugbear,
            SpeciesClass = SpeciesClass.MagicalBeast,
            MovementDelay = 133,
            Weight = 1250,
            Abilities =
                new HashSet<Ability>
                {
                    new Ability
                    {
                        Activation = AbilityActivation.OnMeleeAttack,
                        Action = AbilityAction.Modifier,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                    },
                    new Ability
                    {
                        Activation = AbilityActivation.OnTarget,
                        Action = AbilityAction.Punch,
                        Timeout = 1,
                        Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 1}}
                    }
                },
            SimpleProperties = new HashSet<string> {"infravision", "infravisibility", "humanoidness"},
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Large}, {"physical deflection", 15}},
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            Behavior = MonsterBehavior.WeaponCollector,
            Noise = ActorNoiseType.Growl
        };
    }
}
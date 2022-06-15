using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Erinyes = new Creature
    {
        Name = "erinyes",
        Species = Species.DemonMajor,
        SpeciesClass = SpeciesClass.Demon,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Damage = "50" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Blight { Damage = "10*MightModifier()" } }
            }
        },
        InitialLevel = 7,
        GenerationWeight = "$branch == 'hell' ? 2 : 0",
        GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.SmallGroup,
        Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector,
        Weight = 1000,
        Perception = -6,
        Might = -6,
        Speed = -6,
        Focus = -6,
        Armor = 4,
        MagicResistance = 15,
        FireResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}

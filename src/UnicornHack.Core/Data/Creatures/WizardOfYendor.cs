using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature WizardOfYendor = new Creature
    {
        Name = "Wizard of Yendor",
        Species = Species.Human,
        Abilities = new HashSet<Ability>
        {
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "50*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Spell,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ScriptedEffect { Script = "ArcaneSpell" } }
            }
        },
        InitialLevel = 30,
        GenerationWeight = "0",
        GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.RangedPeaceful | AIBehavior.MagicUser,
        Noise = ActorNoiseType.Cuss,
        Sex = Sex.Male,
        Weight = 1000,
        Perception = 6,
        Focus = 6,
        EnergyRegeneration = 3,
        Regeneration = 3,
        Armor = 9,
        MagicResistance = 50,
        FireResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.Fingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Walking | LocomotionType.Teleportation,
        Telepathic = 3,
        Infravisible = true,
        InvisibilityDetection = true
    };
}

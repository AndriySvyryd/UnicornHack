using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GnomeWizard = new Creature
    {
        Name = "gnome wizard",
        Species = Species.Gnome,
        Abilities = new HashSet<Ability>
        {
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
        InitialLevel = 3,
        GenerationFlags = GenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector | AIBehavior.MagicUser,
        Noise = ActorNoiseType.Speach,
        Sex = Sex.Male,
        Size = 2,
        Weight = 700,
        MovementDelay = 50,
        TurningDelay = 50,
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -8,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature MenwaedsWolf = new Creature
    {
        Name = "Menwaed's Wolf",
        BaseName = "wolf",
        PerceptionBonus = 2,
        MightBonus = 2,
        SpeedBonus = 2,
        FocusBonus = -2,
        Armor = 1,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Holding Bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                EnergyPointCost = 5,
                Effects = new List<Effect> { new PhysicalDamage(), new Bind { Duration = EffectDuration.UntilTimeout, DurationAmount = "100", Amount = "12" } }
            }
        },
        EnglishDescription = "A large wolf with a deep chest, long legs and a rough grey coat darkening along the spine. Its ears stand upright above a narrow muzzle, and its tail hangs stiffly behind it.",
        InitialLevel = 4,
        GenerationWeight = "0"
    };
}

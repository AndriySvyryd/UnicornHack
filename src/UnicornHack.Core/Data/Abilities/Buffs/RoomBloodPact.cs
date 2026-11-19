namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly Ability RoomBloodPact = new Ability
    {
        Name = "room blood pact",
        Type = AbilityType.Buff,
        GenerationWeight = "$feature == 'minor shrine' ? 1.0 : 0.0",
        Activation = ActivationType.Default,
        Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.UntilNextRoom, Name = "Blood Pact" } }
    };
}

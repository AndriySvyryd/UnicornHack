new CreatureVariant
{
    InitialLevel = 20,
    ArmorClass = 6,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    Alignment = -3,
    Name = "kraken",
    Species = Species.Squid,
    MovementRate = 3,
    Size = Size.Huge,
    Weight = 2000,
    Nutrition = 1000,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 12 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Bind { Duration = 7 } }
        }
    }
}

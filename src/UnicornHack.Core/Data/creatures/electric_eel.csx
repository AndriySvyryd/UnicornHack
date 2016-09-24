new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = -3,
    PreviousStageName = "giant eel",
    Name = "electric eel",
    Species = Species.Eel,
    MovementRate = 10,
    Size = Size.Large,
    Weight = 600,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ElectricityResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 14 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Bind { Duration = 10 } }
        }
    }
}

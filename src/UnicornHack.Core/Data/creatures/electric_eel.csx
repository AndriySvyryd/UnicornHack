new CreatureVariant
{
    Name = "electric eel",
    Species = Species.Eel,
    PreviousStageName = "giant eel",
    InitialLevel = 7,
    ArmorClass = -3,
    MovementRate = 10,
    Weight = 600,
    Size = Size.Large,
    Nutrition = 300,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 14 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Bind { Duration = 10 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ElectricityResistance", 3 } }
}

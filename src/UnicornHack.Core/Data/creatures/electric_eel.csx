new Creature
{
    Name = "electric eel",
    Species = Species.Eel,
    ArmorClass = -3,
    MovementRate = 10,
    Weight = 600,
    Size = Size.Large,
    Nutrition = 300,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ElectricityDamage { Damage = 14 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Bind { Duration = 10 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ElectricityResistance", 3 } },
    InitialLevel = 7,
    PreviousStageName = "giant eel"
}

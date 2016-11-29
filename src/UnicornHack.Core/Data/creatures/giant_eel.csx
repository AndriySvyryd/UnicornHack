new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = -1,
    NextStageName = "electric eel",
    Name = "giant eel",
    Species = Species.Eel,
    MovementRate = 9,
    Size = Size.Large,
    Weight = 600,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Bind { Duration = 7 } }
        }
    }
}

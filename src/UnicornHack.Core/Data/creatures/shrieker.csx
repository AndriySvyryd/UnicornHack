new Creature
{
    Name = "shrieker",
    Species = Species.Fungus,
    ArmorClass = 7,
    MovementRate = 1,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 100,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Scream,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Deafen { Duration = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Breathlessness",
        "NonAnimal",
        "Eyelessness",
        "Limblessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    InitialLevel = 3,
    GenerationFrequency = Frequency.Uncommonly
}

new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 7,
    GenerationFrequency = Frequency.Uncommonly,
    Name = "shrieker",
    Species = Species.Fungus,
    MovementRate = 1,
    Size = Size.Small,
    Weight = 100,
    Nutrition = 100,
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
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Scream,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Deafen { Duration = 3 } }
        }
    }
}

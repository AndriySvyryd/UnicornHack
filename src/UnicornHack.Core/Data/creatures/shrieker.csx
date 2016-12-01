new CreatureVariant
{
    Name = "shrieker",
    Species = Species.Fungus,
    InitialLevel = 3,
    ArmorClass = 7,
    MovementRate = 1,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 100,
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
    GenerationFrequency = Frequency.Uncommonly
}

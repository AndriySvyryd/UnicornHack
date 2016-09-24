new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 9,
    GenerationFrequency = Frequency.Commonly,
    Name = "lichen",
    Species = Species.Fungus,
    MovementRate = 1,
    Size = Size.Small,
    Weight = 20,
    Nutrition = 100,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "DecayResistance",
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
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.Targetted, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Stick() } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Stick() } }
    }
}

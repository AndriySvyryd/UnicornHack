new CreatureVariant
{
    Name = "blue jelly",
    Species = Species.Jelly,
    InitialLevel = 4,
    ArmorClass = 8,
    MagicResistance = 10,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 20,
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new ColdDamage { Damage = 3 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new ColdDamage { Damage = 3 } } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Breathlessness",
        "Amorphism",
        "NonAnimal",
        "Eyelessness",
        "Limblessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ColdResistance", 3 }, { "Stealthiness", 3 } },
    GenerationFrequency = Frequency.Commonly
}

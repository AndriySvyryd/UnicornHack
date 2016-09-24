new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 8,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Commonly,
    Name = "spotted jelly",
    Species = Species.Jelly,
    Size = Size.Small,
    Weight = 100,
    Nutrition = 20,
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
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "AcidResistance", 3 }, { "StoningResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new AcidDamage { Damage = 3 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new AcidDamage { Damage = 3 } } }
    }
}

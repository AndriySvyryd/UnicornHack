new Creature
{
    Name = "blue jelly",
    Species = Species.Jelly,
    ArmorClass = 8,
    MagicResistance = 10,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 20,
    Abilities = new HashSet<Ability>
    {
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new ColdDamage { Damage = 3 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new ColdDamage { Damage = 3 } } }
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
    InitialLevel = 4,
    GenerationFrequency = Frequency.Commonly
}

new Creature
{
    Name = "green mold",
    Species = Species.Fungus,
    ArmorClass = 9,
    Weight = 50,
    Size = Size.Small,
    Nutrition = 30,
    Abilities = new HashSet<Ability>
    {
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new AcidDamage { Damage = 3 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new AcidDamage { Damage = 2 } } }
    }
,
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
        "NoInventory",
        "StoningResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "AcidResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "Stealthiness", 3 } },
    InitialLevel = 1,
    GenerationFrequency = Frequency.Uncommonly
}

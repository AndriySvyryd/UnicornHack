new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 10,
    GenerationFrequency = Frequency.Sometimes,
    CorpseVariantName = "",
    Name = "gas spore",
    Species = Species.FloatingSphere,
    MovementRate = 3,
    Size = Size.Small,
    Weight = 10,
    Nutrition = 10,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
        "NonAnimal",
        "Breathlessness",
        "Limblessness",
        "Eyelessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "SlimingResistance", 3 }, { "SicknessResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Explosion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 14 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Explosion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Deafen { Duration = 27 } }
        }
    }
}

new Creature
{
    Name = "paper golem",
    Species = Species.Golem,
    ArmorClass = 10,
    MovementRate = 12,
    Weight = 400,
    Size = Size.Large,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "NonAnimal",
        "Breathlessness",
        "Mindlessness",
        "Humanoidness",
        "Asexuality",
        "StoningResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "WaterWeakness", 3 }, { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "MaxHP", 20 } },
    InitialLevel = 2,
    CorpseName = "",
    GenerationFrequency = Frequency.Rarely
}

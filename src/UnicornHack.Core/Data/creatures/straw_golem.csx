new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 10,
    GenerationFrequency = Frequency.Rarely,
    CorpseVariantName = "",
    Name = "straw golem",
    Species = Species.Golem,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 400,
    SimpleProperties = new HashSet<string> { "SleepResistance", "NonAnimal", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "ColdResistance",
            3
        },
        {
            "PoisonResistance",
            3
        },
        {
            "VenomResistance",
            3
        },
        {
            "StoningResistance",
            3
        },
        {
            "SicknessResistance",
            3
        },
        {
            "MaxHP",
            20
        }
    }
,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

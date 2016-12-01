new CreatureVariant
{
    Name = "gold golem",
    Species = Species.Golem,
    CorpseVariantName = "",
    InitialLevel = 5,
    ArmorClass = 6,
    MovementRate = 9,
    Weight = 2000,
    Size = Size.Medium,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
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
        "SlimingResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "AcidResistance", 3 }, { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 }, { "MaxHP", 40 } },
    GenerationFrequency = Frequency.Rarely
}

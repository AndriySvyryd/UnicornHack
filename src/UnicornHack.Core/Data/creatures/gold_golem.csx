new Creature
{
    Name = "gold golem",
    Species = Species.Golem,
    ArmorClass = 6,
    MovementDelay = 133,
    Weight = 2000,
    Size = Size.Medium,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
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
    InitialLevel = 5,
    CorpseName = "",
    GenerationFrequency = Frequency.Rarely
}

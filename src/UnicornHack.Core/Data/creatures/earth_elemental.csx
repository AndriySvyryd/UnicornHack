new Creature
{
    Name = "earth elemental",
    Species = Species.Elemental,
    SpeciesClass = SpeciesClass.Extraplanar,
    ArmorClass = 2,
    MagicResistance = 30,
    MovementDelay = 200,
    Weight = 2500,
    Size = Size.Huge,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 14 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Stun { Duration = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Phasing",
        "NonAnimal",
        "Breathlessness",
        "Limblessness",
        "Eyelessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "StoningResistance",
        "SlimingResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    InitialLevel = 8,
    CorpseName = "",
    GenerationFrequency = Frequency.Uncommonly
}

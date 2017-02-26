new Creature
{
    Name = "fog cloud",
    Species = Species.Cloud,
    SpeciesClass = SpeciesClass.Extraplanar,
    MagicResistance = 30,
    MovementDelay = 1200,
    Weight = 1,
    Size = Size.Huge,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Engulf { Duration = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new HashSet<Effect> { new WaterDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new WaterDamage { Damage = 2 } } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
        "NonAnimal",
        "NonSolidBody",
        "Breathlessness",
        "Limblessness",
        "Eyelessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "NoInventory",
        "StoningResistance",
        "SlimingResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "AcidResistance", 3 }, { "Stealthiness", 3 } },
    InitialLevel = 3,
    CorpseName = "",
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Sometimes
}

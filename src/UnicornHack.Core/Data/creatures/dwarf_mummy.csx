new Creature
{
    Name = "dwarf mummy",
    Species = Species.Dwarf,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 5,
    MagicResistance = 30,
    MovementDelay = 200,
    Weight = 900,
    Size = Size.Medium,
    Nutrition = 200,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 5,
    CorpseName = "dwarf",
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -4,
    Noise = ActorNoiseType.Moan
}

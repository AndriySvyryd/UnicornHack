new Creature
{
    Name = "elf mummy",
    Species = Species.Elf,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 4,
    MagicResistance = 30,
    MovementDelay = 100,
    Weight = 800,
    Size = Size.Medium,
    Nutrition = 150,
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
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "InvisibilityDetection", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 6,
    CorpseName = "elf",
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -3,
    Noise = ActorNoiseType.Moan
}

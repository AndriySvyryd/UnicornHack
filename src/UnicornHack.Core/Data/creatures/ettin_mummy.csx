new Creature
{
    Name = "ettin mummy",
    Species = Species.Giant,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 1,
    MagicResistance = 20,
    MovementDelay = 100,
    Weight = 2250,
    Size = Size.Huge,
    Nutrition = 350,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 12,
    CorpseName = "ettin",
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -4,
    Noise = ActorNoiseType.Moan
}

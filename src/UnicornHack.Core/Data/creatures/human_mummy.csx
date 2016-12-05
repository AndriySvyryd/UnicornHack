new Creature
{
    Name = "human mummy",
    Species = Species.Human,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 4,
    MagicResistance = 20,
    MovementRate = 12,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 200,
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
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 6,
    CorpseName = "human",
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -5,
    Noise = ActorNoiseType.Moan
}

new Creature
{
    Name = "gnome mummy",
    Species = Species.Gnome,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 6,
    MagicResistance = 20,
    MovementRate = 6,
    Weight = 650,
    Size = Size.Small,
    Nutrition = 100,
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
    InitialLevel = 4,
    CorpseName = "gnome",
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -3,
    Noise = ActorNoiseType.Moan
}

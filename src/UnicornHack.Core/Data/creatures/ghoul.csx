new Creature
{
    Name = "ghoul",
    Species = Species.Ghoul,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 4,
    MovementRate = 8,
    Weight = 400,
    Size = Size.Medium,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Slow { Duration = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "Carnivorism", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 12,
    NextStageName = "ghast",
    CorpseName = "",
    GenerationFrequency = Frequency.Occasionally,
    Alignment = -2,
    Noise = ActorNoiseType.Growl
}

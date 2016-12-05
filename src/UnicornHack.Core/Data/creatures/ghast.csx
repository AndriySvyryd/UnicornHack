new Creature
{
    Name = "ghast",
    Species = Species.Ghoul,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 2,
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
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Paralyze { Duration = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "Carnivorism", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 15,
    PreviousStageName = "ghoul",
    CorpseName = "",
    GenerationFrequency = Frequency.Occasionally,
    Alignment = -3,
    Noise = ActorNoiseType.Growl
}

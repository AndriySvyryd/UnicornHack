new CreatureVariant
{
    InitialLevel = 12,
    ArmorClass = 4,
    GenerationFrequency = Frequency.Occasionally,
    Alignment = -2,
    Noise = ActorNoiseType.Growl,
    CorpseVariantName = "",
    NextStageName = "ghast",
    Name = "ghoul",
    Species = Species.Ghoul,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 8,
    Size = Size.Medium,
    Weight = 400,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "Carnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Slow { Duration = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
}

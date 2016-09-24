new CreatureVariant
{
    InitialLevel = 15,
    ArmorClass = 2,
    GenerationFrequency = Frequency.Occasionally,
    Alignment = -3,
    Noise = ActorNoiseType.Growl,
    CorpseVariantName = "",
    PreviousStageName = "ghoul",
    Name = "ghast",
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
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Paralyze { Duration = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
}

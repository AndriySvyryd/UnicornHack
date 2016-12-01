new CreatureVariant
{
    Name = "ghast",
    Species = Species.Ghoul,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "",
    PreviousStageName = "ghoul",
    InitialLevel = 15,
    ArmorClass = 2,
    MovementRate = 8,
    Weight = 400,
    Size = Size.Medium,
    Nutrition = 50,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Paralyze { Duration = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "Carnivorism", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Occasionally,
    Alignment = -3,
    Noise = ActorNoiseType.Growl
}

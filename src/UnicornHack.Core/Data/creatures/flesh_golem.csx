new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = 9,
    GenerationFrequency = Frequency.Rarely,
    Name = "flesh golem",
    Species = Species.Golem,
    MovementRate = 8,
    Size = Size.Large,
    Weight = 1400,
    Nutrition = 600,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 }, { "MaxHP", 40 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
    }
}

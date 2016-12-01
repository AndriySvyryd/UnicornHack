new CreatureVariant
{
    Name = "flesh golem",
    Species = Species.Golem,
    InitialLevel = 9,
    ArmorClass = 9,
    MovementRate = 8,
    Weight = 1400,
    Size = Size.Large,
    Nutrition = 600,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 }, { "MaxHP", 40 } },
    GenerationFrequency = Frequency.Rarely
}

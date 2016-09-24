new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Rarely,
    CorpseVariantName = "",
    Name = "leather golem",
    Species = Species.Golem,
    MovementRate = 6,
    Size = Size.Large,
    Weight = 800,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "MaxHP", 40 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}

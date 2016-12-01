new CreatureVariant
{
    Name = "leather golem",
    Species = Species.Golem,
    CorpseVariantName = "",
    InitialLevel = 6,
    ArmorClass = 6,
    MovementRate = 6,
    Weight = 800,
    Size = Size.Large,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Breathlessness", "Mindlessness", "Humanoidness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "MaxHP", 40 } },
    GenerationFrequency = Frequency.Rarely
}

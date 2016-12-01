new CreatureVariant
{
    Name = "skeleton",
    Species = Species.Skeleton,
    SpeciesClass = SpeciesClass.Undead,
    CorpseVariantName = "",
    InitialLevel = 3,
    ArmorClass = 10,
    MovementRate = 6,
    Weight = 300,
    Size = Size.Medium,
    Nutrition = 5,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Infravision",
        "Humanoidness",
        "Breathlessness",
        "Mindlessness",
        "StoningResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.WeaponCollector
}

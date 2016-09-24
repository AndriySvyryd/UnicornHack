new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 10,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.WeaponCollector,
    CorpseVariantName = "",
    Name = "skeleton",
    Species = Species.Skeleton,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 6,
    Size = Size.Medium,
    Weight = 300,
    Nutrition = 5,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "Mindlessness" },
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "ColdResistance",
            3
        },
        {
            "PoisonResistance",
            3
        },
        {
            "StoningResistance",
            3
        },
        {
            "SicknessResistance",
            3
        },
        {
            "VenomResistance",
            3
        },
        {
            "ThickHide",
            3
        }
    }
,
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
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

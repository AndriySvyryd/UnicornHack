new CreatureVariant
{
    InitialLevel = 25,
    ArmorClass = -6,
    MagicResistance = 90,
    GenerationFlags = GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
    Alignment = -15,
    Noise = ActorNoiseType.Mumble,
    CorpseVariantName = "",
    PreviousStageName = "master lich",
    Name = "arch-lich",
    Species = Species.Lich,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 9,
    Size = Size.Medium,
    Weight = 600,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness" },
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "FireResistance",
            3
        },
        {
            "ColdResistance",
            3
        },
        {
            "ElectricityResistance",
            3
        },
        {
            "SicknessResistance",
            3
        },
        {
            "PoisonResistance",
            3
        },
        {
            "VenomResistance",
            3
        },
        {
            "Regeneration",
            3
        }
    }
,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ColdDamage { Damage = 17 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 18 } } }
    }
}

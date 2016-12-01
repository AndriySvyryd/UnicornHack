new CreatureVariant
{
    Name = "archon",
    Species = Species.Angel,
    SpeciesClass = SpeciesClass.Celestial,
    CorpseVariantName = "",
    PreviousStageName = "angel",
    InitialLevel = 19,
    ArmorClass = -6,
    MagicResistance = 80,
    MovementRate = 16,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new MagicalDamage { Damage = 14 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Flight", "FlightControl", "Infravisibility", "Infravision", "InvisibilityDetection", "Humanoidness" },
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
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Alignment = 15,
    Noise = ActorNoiseType.Speach
}

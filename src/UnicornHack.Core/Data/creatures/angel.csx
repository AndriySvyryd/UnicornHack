new CreatureVariant
{
    InitialLevel = 14,
    ArmorClass = -4,
    MagicResistance = 55,
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Alignment = 12,
    Noise = ActorNoiseType.Speach,
    CorpseVariantName = "",
    PreviousStageName = "aleax",
    NextStageName = "archon",
    Name = "angel",
    Species = Species.Angel,
    SpeciesClass = SpeciesClass.Celestial,
    MovementRate = 10,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Flight", "FlightControl", "Infravisibility", "Infravision", "InvisibilityDetection", "Humanoidness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "ElectricityResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
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
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new MagicalDamage { Damage = 7 } }
        }
    }
}

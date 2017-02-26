new Creature
{
    Name = "angel",
    Species = Species.Angel,
    SpeciesClass = SpeciesClass.Celestial,
    ArmorClass = -4,
    MagicResistance = 55,
    MovementDelay = 120,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new HashSet<Effect> { new MagicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Flight", "FlightControl", "Infravisibility", "Infravision", "InvisibilityDetection", "Humanoidness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "ElectricityResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    InitialLevel = 14,
    PreviousStageName = "aleax",
    NextStageName = "archon",
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Alignment = 12,
    Noise = ActorNoiseType.Speach
}

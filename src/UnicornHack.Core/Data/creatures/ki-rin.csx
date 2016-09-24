new CreatureVariant
{
    InitialLevel = 16,
    ArmorClass = -5,
    MagicResistance = 90,
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking,
    Alignment = 15,
    Noise = ActorNoiseType.Neigh,
    CorpseVariantName = "",
    Name = "ki-rin",
    Species = Species.Kirin,
    SpeciesClass = SpeciesClass.Reptile | SpeciesClass.Celestial,
    MovementRate = 18,
    Size = Size.Large,
    Weight = 1300,
    Nutrition = 600,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
        "AnimalBody",
        "Infravisibility",
        "Infravision",
        "InvisibilityDetection",
        "Handlessness",
        "SingularInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
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

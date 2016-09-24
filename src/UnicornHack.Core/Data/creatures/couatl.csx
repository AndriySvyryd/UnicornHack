new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 5,
    MagicResistance = 30,
    GenerationFlags = GenerationFlags.SmallGroup | GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking,
    Alignment = 7,
    Noise = ActorNoiseType.Hiss,
    CorpseVariantName = "",
    Name = "couatl",
    Species = Species.WingedSnake,
    SpeciesClass = SpeciesClass.Reptile | SpeciesClass.Celestial,
    MovementRate = 10,
    Size = Size.Large,
    Weight = 900,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravision", "SerpentlikeBody", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Bind { Duration = 7 } }
        }
    }
}

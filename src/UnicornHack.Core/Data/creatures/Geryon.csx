new Creature
{
    Name = "Geryon",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    ArmorClass = -3,
    MagicResistance = 75,
    MovementDelay = 400,
    Weight = 2500,
    Size = Size.Huge,
    Nutrition = 500,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 13 } } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "Infravisibility",
        "Infravision",
        "InvisibilityDetection",
        "SerpentlikeBody",
        "HumanoidTorso",
        "Maleness",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 72,
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.Covetous | MonsterBehavior.Bribeable,
    Alignment = -15,
    Noise = ActorNoiseType.Bribe
}

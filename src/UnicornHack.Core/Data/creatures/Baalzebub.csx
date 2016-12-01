new CreatureVariant
{
    Name = "Baalzebub",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    CorpseVariantName = "",
    InitialLevel = 89,
    ArmorClass = -5,
    MagicResistance = 85,
    MovementRate = 9,
    Weight = 1500,
    Size = Size.Large,
    Nutrition = 500,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Stun { Duration = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 14 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "Infravision", "InvisibilityDetection", "Maleness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 } },
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.Covetous | MonsterBehavior.Bribeable,
    Alignment = -20,
    Noise = ActorNoiseType.Bribe
}

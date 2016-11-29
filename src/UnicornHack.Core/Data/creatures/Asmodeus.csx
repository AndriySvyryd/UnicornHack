new CreatureVariant
{
    InitialLevel = 105,
    ArmorClass = -7,
    MagicResistance = 90,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.Covetous | MonsterBehavior.Bribeable,
    Alignment = -20,
    Noise = ActorNoiseType.Bribe,
    CorpseVariantName = "",
    Name = "Asmodeus",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 1500,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "Infravision", "InvisibilityDetection", "Humanoidness", "Maleness" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ColdDamage { Damage = 21 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 18 } } }
    }
}

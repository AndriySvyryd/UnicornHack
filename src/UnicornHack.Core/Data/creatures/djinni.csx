new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 4,
    MagicResistance = 30,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Djinni,
    CorpseVariantName = "",
    Name = "djinni",
    Species = Species.Djinni,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1400,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "Humanoidness" },
    ValuedProperties = new Dictionary<string, Object> { { "StoningResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
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
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
}

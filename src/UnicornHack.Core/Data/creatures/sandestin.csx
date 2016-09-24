new CreatureVariant
{
    InitialLevel = 13,
    ArmorClass = 4,
    MagicResistance = 60,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
    Alignment = -5,
    Noise = ActorNoiseType.Cuss,
    CorpseVariantName = "",
    Name = "sandestin",
    Species = Species.Sandestin,
    SpeciesClass = SpeciesClass.ShapeChanger,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1500,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness" },
    ValuedProperties = new Dictionary<string, Object> { { "StoningResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
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
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 4 } } }
    }
}

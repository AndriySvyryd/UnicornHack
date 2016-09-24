new CreatureVariant
{
    InitialLevel = 78,
    ArmorClass = -2,
    MagicResistance = 80,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser | MonsterBehavior.Covetous | MonsterBehavior.Bribeable,
    Alignment = -15,
    Noise = ActorNoiseType.Bribe,
    CorpseVariantName = "",
    Name = "Dispater",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 15,
    Size = Size.Large,
    Weight = 1500,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "Infravision", "InvisibilityDetection", "Humanoidness", "Maleness" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
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
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 13 } } }
    }
}

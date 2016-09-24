new CreatureVariant
{
    InitialLevel = 66,
    ArmorClass = -6,
    MagicResistance = 85,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
    Alignment = -20,
    Noise = ActorNoiseType.Grunt,
    CorpseVariantName = "",
    Name = "Orcus",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 9,
    Size = Size.Huge,
    Weight = 2500,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "Infravision", "InvisibilityDetection", "Humanoidness", "Maleness" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "SicknessResistance", 3 } },
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
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 5 } }
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
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 13 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { Strength = 13 } } }
    }
}
